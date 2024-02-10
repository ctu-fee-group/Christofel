//
//   UserCommandsGroup.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Extensions;
using Christofel.CommandsLib.Permissions;
using Christofel.CommandsLib.Validator;
using Christofel.Common.Database;
using Christofel.Common.Database.Models;
using Christofel.Common.User;
using Christofel.CtuAuth;
using Christofel.Management.CtuUtils;
using Christofel.OAuth;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.Management.Commands
{
    /// <summary>
    /// Command group that handles /users commands.
    /// </summary>
    [Group("users")]
    [RequirePermission("management.users")]
    [Description("Manage users and their identities")]
    [Ephemeral]
    public class UserCommandsGroup : CommandGroup
    {
        private readonly ICommandContext _context;
        private readonly IServiceProvider _services;
        private readonly ChristofelBaseContext _dbContext;
        private readonly FeedbackService _feedbackService;

        private readonly CtuIdentityResolver _identityResolver;

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserCommandsGroup"/> class.
        /// </summary>
        /// <param name="feedbackService">The feedback service.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="identityResolver">The identity resolver.</param>
        /// <param name="dbContext">The christofel base database context.</param>
        /// <param name="context">The context of the current command.</param>
        /// <param name="services">The service provider.</param>
        public UserCommandsGroup
        (
            FeedbackService feedbackService,
            ILogger<UserCommandsGroup> logger,
            CtuIdentityResolver identityResolver,
            ChristofelBaseContext dbContext,
            ICommandContext context,
            IServiceProvider services
        )
        {
            _context = context;
            _services = services;
            _feedbackService = feedbackService;
            _identityResolver = identityResolver;
            _logger = logger;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Handles /users auth.
        /// </summary>
        /// <param name="user">The user to authenticate.</param>
        /// <param name="ctuUsername">The username of the link user.</param>
        /// <returns>A result that may not have succeeded.</returns>
        [Command("auth")]
        [Description("Trigger authentication of the given user.")]
        [RequirePermission("management.users.auth")]
        public async Task<Result> HandleAuthUser
        (
            [Description("The discord user to authenticate")] [DiscordTypeHint(TypeHint.User)]
            Snowflake user,
            [Description("CTU username of the user to add")]
            string ctuUsername
        )
        {
            var auth = _services.GetRequiredService<CtuAuthProcess>();
            var guildApi = _services.GetRequiredService<IDiscordRestGuildAPI>();
            var options = _services.GetRequiredService<IOptionsSnapshot<BotOptions>>().Value;
            var ctuUser = new CtuUser(ctuUsername);

            var memberResult = await guildApi.GetGuildMemberAsync
            (
                new Snowflake(options.GuildId, Constants.DiscordEpoch),
                user,
                CancellationToken
            );

            if (!memberResult.IsDefined(out var member))
            {
                var feedbackResponse = await _feedbackService.SendContextualSuccessAsync
                    ("There was an error while retrieving the guild member from Discord API.");

                return feedbackResponse.IsSuccess
                    ? Result.FromSuccess()
                    : Result.FromError(feedbackResponse);
            }

            var dbUser = new DbUser
            {
                CtuUsername = ctuUsername,
                DiscordId = user,
                AuthenticatedAt = DateTime.Now
            };

            try
            {
                _dbContext.Add(dbUser);
                await _dbContext.SaveChangesAsync(CancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "There was an error while saving data to the database");

                var feedbackResponse = await _feedbackService.SendContextualSuccessAsync
                    ("There was an error while saving data to the database");

                return feedbackResponse.IsSuccess
                    ? Result.FromSuccess()
                    : Result.FromError(feedbackResponse);
            }

            var authResult = await auth.FinishAuthAsync
                (ctuUser, _dbContext, options.GuildId, dbUser, member, CancellationToken);

            if (!authResult.IsSuccess)
            {
                await _feedbackService.SendContextualErrorAsync("There was an error during the authentication: " + authResult.Error);
                return authResult;
            }

            var feedbackResult = await _feedbackService.SendContextualInfoAsync("Successfully authenticated the user!");

            return feedbackResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(feedbackResult);
        }

        private record CtuUser(string CtuUsername) : ICtuUser;

        /// <summary>
        /// Handles /users add.
        /// </summary>
        /// <param name="user">Discord id of the user to add to the database.</param>
        /// <param name="ctuUsername">Username of the user.</param>
        /// <returns>A result that may not have succeeded.</returns>
        [Command("add")]
        [Description("Add user to database manually")]
        [RequirePermission("management.users.add")]
        public async Task<Result> HandleAddUser
        (
            [Description("Discord user to add")] [DiscordTypeHint(TypeHint.User)]
            Snowflake user,
            [Description("CTU Username of the user to add")]
            string ctuUsername
        )
        {
            DbUser dbUser = new DbUser
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
                    await _feedbackService.SendContextualSuccessAsync
                        ($"New user <@{user}> added. You have to assign him roles manually");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "There was an error while saving data to the database");
                feedbackResponse =
                    await _feedbackService.SendContextualSuccessAsync
                        ("There was an error while saving data to the database");
            }

            return feedbackResponse.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(feedbackResponse);
        }

        /// <summary>
        /// Handles /users showidentity.
        /// </summary>
        /// <remarks>
        /// Shows identity of the given user. The user will be notified about this action.
        /// </remarks>
        /// <param name="user">Discord id of the user to show identity of. Specified as User in slash command.</param>
        /// <param name="discordId">Discord id of the user to show identity of. Specified as string in slash command.</param>
        /// <returns>A result that may not have succeeded.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the validation has failed due to programmers error.</exception>
        [Command("showidentity")]
        [Description
            ("Show identity of a user. The user will be notified about this and your identity will be shown to him")]
        [RequirePermission("management.users.showidentity")]
        public async Task<Result> HandleShowIdentity
        (
            [Description("Show identity of this user")] [DiscordTypeHint(TypeHint.User)]
            Snowflake? user = null,
            [Description("Show identity of this user based on discord id (userful for deleted accounts)")]
            [DiscordTypeHint(TypeHint.String)]
            Snowflake? discordId = null
        )
        {
            var validationResult = ExactlyOneValidation("user, discordid", user, discordId);
            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            try
            {
                var userId = user ?? discordId ?? throw new InvalidOperationException("Validation failed");

                var identities =
                    (await _identityResolver.GetIdentitiesCtuUsernamesList(userId))
                    .Select(x => $@"CTU username: {x}")
                    .ToList();

                string response = identities.Count switch
                {
                    0 => "Could not find this user in database (he may not be authenticated or is not in database)",
                    1 => "Found exactly one identity for this user: ",
                    _ => "Found multiple identities for this user: ",
                };
                var notifyUser = identities.Count != 0;

                response += string.Join(", ", identities);

                var feedbackResult = await _feedbackService.SendContextualSuccessAsync(response, ct: CancellationToken);

                if (!feedbackResult.IsSuccess)
                {
                    return Result.FromError(feedbackResult);
                }

                if (!_context.TryGetUserID(out var executingUserId))
                {
                    return (Result)new GenericError("Could not get user id from context.");
                }

                if (notifyUser)
                {
                    try
                    {
                        var commandUserIdentity =
                            await _identityResolver.GetFirstIdentity(executingUserId);
                        var dmFeedbackResult = await _feedbackService.SendPrivateNeutralAsync
                        (
                            userId,
                            $@"Ahoj, uživatel {commandUserIdentity?.CtuUsername ?? "(ČVUT údaje nebyly nalezeny)"} alias {_context.GetUserDiscordHandleOrDefault()} právě zjišťoval tvůj username. Pokud máš pocit, že došlo ke zneužití, kontaktuj podporu."
                        );

                        if (!dmFeedbackResult.IsSuccess)
                        {
                            return Result.FromError(dmFeedbackResult);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError
                        (
                            e,
                            "Could not send DM message to the user notifying him about being identified"
                        );
                        await _feedbackService.SendContextualErrorAsync
                            ("Could not send DM message to the user notifying him about being identified");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not get the user from the database");
                var feedbackResult = await _feedbackService.SendContextualErrorAsync
                    ("Could not get the user from the database");

                if (!feedbackResult.IsSuccess)
                {
                    return Result.FromError(feedbackResult);
                }
            }

            return Result.FromSuccess();
        }

        private Result ExactlyOneValidation<TLeft, TRight>(string name, TLeft? left, TRight? right)
        {
            var validationResult = new CommandValidator()
                .MakeSure
                (
                    name,
                    (left, right),
                    o => o
                        .Must(x => x.left is not null ^ x.right is not null)
                        .WithMessage("Exactly one must be specified.")
                )
                .Validate()
                .GetResult();

            return validationResult;
        }

        /// <summary>
        /// Handles /users duplicate commands.
        /// </summary>
        [Group("duplicate")]
        [Description("Manage user duplicates")]
        [RequirePermission("management.users.duplicate")]
        public class InnerDuplicate : CommandGroup
        {
            private readonly ChristofelBaseContext _dbContext;
            private readonly FeedbackService _feedbackService;
            private readonly CtuIdentityResolver _identityResolver;
            private readonly ILogger _logger;
            private readonly IDiscordRestUserAPI _userApi;
            private readonly UsersOptions _usersOptions;

            /// <summary>
            /// Initializes a new instance of the <see cref="InnerDuplicate"/> class.
            /// </summary>
            /// <param name="feedbackService">The feedback service.</param>
            /// <param name="logger">The logger.</param>
            /// <param name="userApi">The user api.</param>
            /// <param name="dbContext">The christofel base database context.</param>
            /// <param name="usersOptions">The user options.</param>
            /// <param name="identityResolver">The identity resolver.</param>
            public InnerDuplicate
            (
                FeedbackService feedbackService,
                ILogger<UserCommandsGroup> logger,
                IDiscordRestUserAPI userApi,
                ChristofelBaseContext dbContext,
                IOptionsSnapshot<UsersOptions> usersOptions,
                CtuIdentityResolver identityResolver
            )
            {
                _usersOptions = usersOptions.Value;
                _userApi = userApi;
                _feedbackService = feedbackService;
                _identityResolver = identityResolver;
                _logger = logger;
                _dbContext = dbContext;
            }

            /// <summary>
            /// Handles /users duplicity allow command.
            /// </summary>
            /// <param name="user">The user to allow duplicate to.</param>
            /// <returns>A result that may not have succeeded.</returns>
            [Command("allow")]
            [Description("Allow duplicate user to be registered")]
            [RequirePermission("management.users.duplicate.allow")]
            public async Task<Result> HandleAllowDuplicity
            (
                [Description("Allow duplicate registration to this specified duplicate user")]
                IUser user
            )
            {
                Result<IReadOnlyList<IMessage>> feedbackResult;
                try
                {
                    var dbUser =
                        await _dbContext.Users.FirstOrDefaultAsync
                        (
                            x => x.DiscordId == user.ID && x.DuplicitUserId != null && x.AuthenticatedAt == null,
                            CancellationToken
                        );

                    if (dbUser == null)
                    {
                        feedbackResult =
                            await _feedbackService.SendContextualErrorAsync
                                ("The given user is not in database or is not a duplicity", ct: CancellationToken);
                    }
                    else
                    {
                        var code = dbUser.RegistrationCode;
                        var link = _usersOptions.AuthLink.Replace("{code}", code);
                        dbUser.DuplicityApproved = true;

                        await _dbContext.SaveChangesAsync(CancellationToken);
                        feedbackResult =
                            await _feedbackService.SendContextualSuccessAsync
                            (
                                $"Duplicity approved. Link for authentication is: {link}",
                                ct: CancellationToken
                            );
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Could not get the given user from database or the changes could not be saved");
                    feedbackResult =
                        await _feedbackService.SendContextualSuccessAsync
                        (
                            "Could not get the given user from database or the changes could not be saved",
                            ct: CancellationToken
                        );
                }

                return feedbackResult.IsSuccess
                    ? Result.FromSuccess()
                    : Result.FromError(feedbackResult);
            }

            /// <summary>
            /// Handles /users duplicate show.
            /// </summary>
            /// <param name="user">The user to show duplicates of.</param>
            /// <returns>A result that may not have succeeded.</returns>
            [Command("show")]
            [Description("Show information about specified user duplicates")]
            [RequirePermission("management.users.duplicate.show")]
            public async Task<Result> HandleShowDuplicity
            (
                [Description("Show duplicate information of the specified user")]
                IUser user
            )
            {
                Result<IReadOnlyList<IMessage>> feedbackResult;
                List<Embed> embeds = new List<Embed>();
                try
                {
                    List<Snowflake> duplicities =
                        await _identityResolver.GetDuplicitiesDiscordIdsList(user.ID, CancellationToken);

                    foreach (var targetUser in duplicities)
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
                                new EmbedAuthor
                                (
                                    $"{currentUser.Username}#{currentUser.Discriminator} <@{currentUser.ID.Value}>",
                                    IconUrl: avatar.IsSuccess ? avatar.Entity.ToString() : string.Empty
                                );
                        }

                        embeds.Add
                        (
                            new Embed
                            (
                                "Duplicity",
                                Description: "This is a duplicity of the specified user.",
                                Footer: new EmbedFooter
                                    ($"For identity of this user, use /users showidentity discordid:<@{targetUser}>"),
                                Author: embedAuthor
                            )
                        );
                    }

                    if (embeds.Count == 0)
                    {
                        feedbackResult =
                            await _feedbackService.SendContextualNeutralAsync
                                ("Could not find any duplicate records of the given user");
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
    }
}