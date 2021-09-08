using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.User;
using Christofel.CommandsLib;
using Christofel.CommandsLib.Validator;
using Christofel.Management.CtuUtils;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Core;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using IUser = Remora.Discord.API.Abstractions.Objects.IUser;

namespace Christofel.Management.Commands
{
    [Group("users")]
    [RequirePermission("management.users")]
    [Description("Manage users and their identities")]
    [Ephemeral]
    public class UserCommandsGroup : CommandGroup
    {
        // /users add @user ctuUsername
        // /users showidentity @user or discordId
        // /users duplicate allow @user
        //   - respond who is the duplicity
        //   - respond with auth link
        // /users duplicate show @user
        //   - show duplicate information

        private readonly ILogger<MessageCommandsGroup> _logger;
        private readonly CtuIdentityResolver _identityResolver;
        private readonly FeedbackService _feedbackService;
        private readonly ChristofelBaseContext _dbContext;
        private readonly ICommandContext _context;

        public UserCommandsGroup(FeedbackService feedbackService,
            ILogger<MessageCommandsGroup> logger,
            CtuIdentityResolver identityResolver,
            ChristofelBaseContext dbContext,
            ICommandContext context)
        {
            _context = context;
            _feedbackService = feedbackService;
            _identityResolver = identityResolver;
            _logger = logger;
            _dbContext = dbContext;
        }

        [Group("duplicate")]
        [Description("Manage user duplicates")]
        [RequirePermission("management.users.duplicate")]
        public class InnerDuplicate : CommandGroup
        {
            private readonly ILogger<MessageCommandsGroup> _logger;
            private readonly ChristofelBaseContext _dbContext;
            private readonly CtuIdentityResolver _identityResolver;
            private readonly FeedbackService _feedbackService;
            private readonly IDiscordRestUserAPI _userApi;

            public InnerDuplicate(FeedbackService feedbackService,
                ILogger<MessageCommandsGroup> logger, IDiscordRestUserAPI userApi,
                ChristofelBaseContext dbContext, CtuIdentityResolver identityResolver)
            {
                _userApi = userApi;
                _feedbackService = feedbackService;
                _identityResolver = identityResolver;
                _logger = logger;
                _dbContext = dbContext;
            }

            [Command("allow")]
            [Description("Allow duplicate user to be registered")]
            [RequirePermission("management.users.duplicate.allow")]
            public async Task<Result> HandleAllowDuplicity(
                [Description("Allow duplicate registration to this specified duplicate user")]
                IUser user)
            {
                Result<IReadOnlyList<IMessage>> feedbackResult;
                try
                {
                    DbUser? dbUser =
                        await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(_dbContext.Users,
                            x => x.DiscordId == user.ID && x.DuplicitUserId != null && x.AuthenticatedAt == null,
                            CancellationToken);

                    if (dbUser == null)
                    {
                        feedbackResult =
                            await _feedbackService.SendContextualErrorAsync(
                                "The given user is not in database or is not a duplicity", ct: CancellationToken);
                    }
                    else
                    {
                        dbUser.DuplicityApproved = true;

                        await _dbContext.SaveChangesAsync(CancellationToken);
                        feedbackResult =
                            await _feedbackService.SendContextualSuccessAsync(
                                "Duplicity approved. Link for authentication is: **LINK**", ct: CancellationToken);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Could not get the given user from database or the changes could not be saved");
                    feedbackResult =
                        await _feedbackService.SendContextualSuccessAsync(
                            "Could not get the given user from database or the changes could not be saved",
                            ct: CancellationToken);
                }

                return feedbackResult.IsSuccess
                    ? Result.FromSuccess()
                    : Result.FromError(feedbackResult);
            }

            [Command("show")]
            [Description("Show information about specified user duplicates")]
            [RequirePermission("management.users.duplicate.show")]
            public async Task<Result> HandleShowDuplicity(
                [Description("Show duplicate information of the specified user")]
                IUser user)
            {
                Result<IReadOnlyList<IMessage>> feedbackResult;
                List<Embed> embeds = new List<Embed>();
                try
                {
                    List<Snowflake> duplicities =
                        await _identityResolver.GetDuplicitiesDiscordIdsList(user.ID, CancellationToken);

                    foreach (Snowflake targetUser in duplicities)
                    {
                        Optional<IEmbedAuthor> embedAuthor = default;

                        var currentUserResult = await _userApi.GetUserAsync(targetUser);
                        if (!currentUserResult.IsSuccess)
                        {
                            embedAuthor =
                                new EmbedAuthor($@"Could not find discord mapping - known discord id: {targetUser}");
                        }
                        else
                        {
                            var currentUser = currentUserResult.Entity;

                            var avatar = CDN.GetUserAvatarUrl(currentUserResult.Entity);
                            if (!avatar.IsSuccess)
                            {
                                avatar = CDN.GetDefaultUserAvatarUrl(currentUserResult.Entity);
                            }
                            
                            embedAuthor =
                                new EmbedAuthor(
                                    $"{currentUser.Username}#{currentUser.Discriminator} <@{currentUser.ID.Value}>",
                                    IconUrl: (avatar.IsSuccess ? avatar.Entity.ToString() : string.Empty));
                        }

                        embeds.Add(new Embed(Title: "Duplicity",
                            Description: "This is a duplicity of the specified user.",
                            Footer: new EmbedFooter(
                                $"For identity of this user, use /users showidentity discordid:<@{targetUser}>"),
                            Author: embedAuthor));
                    }

                    if (embeds.Count == 0)
                    {
                        feedbackResult =
                            await _feedbackService.SendContextualNeutralAsync(
                                "Could not find any duplicate records of the given user");
                    }
                    else
                    {
                        foreach (var embed in embeds)
                        {
                            // TODO: group to one message?
                            var result =
                                await _feedbackService.SendContextualEmbedAsync(embed, ct: CancellationToken);
                            if (!result.IsSuccess)
                            {
                                return Result.FromError(result);
                            }
                        }

                        return Result.FromSuccess();
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Could not get the users from database.");
                    feedbackResult =
                        await _feedbackService.SendContextualErrorAsync("Could not get the users from database");
                }

                return feedbackResult.IsSuccess
                    ? Result.FromSuccess()
                    : Result.FromError(feedbackResult);
            }
        }

        [Command("add")]
        [Description("Add user to database manually")]
        [RequirePermission("management.users.add")]
        public async Task<Result> HandleAddUser(
            [Description("Discord user to add"), DiscordTypeHint(TypeHint.User)]
            Snowflake user,
            [Description("CTU Username of the user to add")]
            string ctuUsername)
        {
            DbUser dbUser = new DbUser()
            {
                CtuUsername = ctuUsername,
                DiscordId = user,
                AuthenticatedAt = DateTime.Now
            };

            Result<IReadOnlyList<IMessage>> feedbackResponse;
            try
            {
                _dbContext.Add(dbUser);
                await _dbContext.SaveChangesAsync(CancellationToken);

                feedbackResponse =
                    await _feedbackService.SendContextualSuccessAsync(
                        $"New user <@{user}> added. You have to assign him roles manually");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "There was an error while saving data to the database");
                feedbackResponse =
                    await _feedbackService.SendContextualSuccessAsync(
                        $"There was an error while saving data to the database");
            }

            return feedbackResponse.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(feedbackResponse);
        }

        [Command("showidentity")]
        [Description(
            "Show identity of a user. The user will be notified about this and your identity will be shown to him")]
        [RequirePermission("management.users.showidentity")]
        public async Task<Result> HandleShowIdentity(
            [Description("Show identity of this user"), DiscordTypeHint(TypeHint.User)]
            Snowflake? user = null,
            [Description("Show identity of this user based on discord id (userful for deleted accounts)"),
             DiscordTypeHint(TypeHint.String)]
            Snowflake? discordId = null)
        {
            var validationResult = ExactlyOneValidation("user, discordid", user, discordId);
            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            try
            {
                var userId = user ?? discordId ?? throw new InvalidOperationException("Validation failed");

                List<string> identities =
                    (await _identityResolver.GetIdentitiesCtuUsernamesList(userId))
                    .Select(x => $@"CTU username: {x}")
                    .ToList();

                string response = identities.Count switch
                {
                    0 => "Could not find this user in database (he may not be authenticated or is not in database)",
                    1 => "Found exactly one identity for this user: ",
                    _ => "Found multiple identities for this user: "
                };
                bool notifyUser = identities.Count != 0;

                response += string.Join(", ", identities);

                var feedbackResult = await _feedbackService.SendContextualSuccessAsync(response, ct: CancellationToken);

                if (!feedbackResult.IsSuccess)
                {
                    return Result.FromError(feedbackResult);
                }

                if (notifyUser)
                {
                    try
                    {
                        ILinkUser? commandUserIdentity =
                            await _identityResolver.GetFirstIdentity(_context.User.ID);
                        var dmFeedbackResult = await _feedbackService.SendPrivateNeutralAsync(userId,
                            $@"Ahoj, uživatel {commandUserIdentity?.CtuUsername ?? "(ČVUT údaje nebyly nalezeny)"} alias {_context.User.Username}#{_context.User.Discriminator} právě zjišťoval tvůj username. Pokud máš pocit, že došlo ke zneužití, kontaktuj podporu.");

                        if (!dmFeedbackResult.IsSuccess)
                        {
                            return Result.FromError(dmFeedbackResult);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e,
                            "Could not send DM message to the user notifying him about being identified");
                        await _feedbackService.SendContextualErrorAsync(
                            "Could not send DM message to the user notifying him about being identified");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not get the user from the database");
                var feedbackResult = await _feedbackService.SendContextualErrorAsync(
                    "Could not get the user from the database");

                if (!feedbackResult.IsSuccess)
                {
                    return Result.FromError(feedbackResult);
                }
            }
            
            return Result.FromSuccess();
        }

        private Result ExactlyOneValidation<T, U>(string name, T? left, U? right)
        {
            var validationResult = new CommandValidator()
                .MakeSure(name, (left, right),
                    o => o
                        .Must(x => (x.left is not null) ^ (x.right is not null))
                        .WithMessage("Exactly one must be specified."))
                .Validate()
                .GetResult();

            return validationResult;
        }
    }
}