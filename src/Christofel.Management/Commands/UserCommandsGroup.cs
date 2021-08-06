using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.Permissions;
using Christofel.BaseLib.User;
using Christofel.CommandsLib.Commands;
using Christofel.CommandsLib.CommandsInfo;
using Christofel.CommandsLib.Executors;
using Christofel.CommandsLib.HandlerCreator;
using Christofel.CommandsLib.Extensions;
using Christofel.CommandsLib.Verifier;
using Christofel.Management.Commands.Verifiers;
using Christofel.Management.CtuUtils;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IUser = Discord.IUser;

namespace Christofel.Management.Commands
{
    public class UserCommandsGroup : ICommandGroup
    {
        private class UserData : IHasDiscordId
        {
            public ulong? DiscordId { get; set; }
        }

        // /users add @user ctuUsername
        // /users showidentity @user or discordId
        // /duplicity allow @user
        //   - respond who is the duplicity
        //   - respond with auth link
        // /duplicity show @user
        //   - show duplicate information

        private readonly DiscordSocketClient _client;
        private readonly BotOptions _options;
        private readonly ILogger<MessageCommandsGroup> _logger;
        private readonly IPermissionsResolver _resolver;
        private readonly IDbContextFactory<ChristofelBaseContext> _dbContextFactory;
        private readonly CtuIdentityResolver _identityResolver;

        public UserCommandsGroup(IOptions<BotOptions> options, IPermissionsResolver resolver,
            ILogger<MessageCommandsGroup> logger, DiscordSocketClient client,
            IDbContextFactory<ChristofelBaseContext> dbContextFactory, CtuIdentityResolver identityResolver)
        {
            _identityResolver = identityResolver;
            _client = client;
            _options = options.Value;
            _logger = logger;
            _resolver = resolver;
            _dbContextFactory = dbContextFactory;
        }

        public async Task HandleAllowDuplicity(SocketSlashCommand command, IUser user,
            CancellationToken token = default)
        {
            try
            {
                await using ChristofelBaseContext context = _dbContextFactory.CreateDbContext();
                DbUser? dbUser =
                    await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(context.Users,
                        x => x.DiscordId == user.Id && x.DuplicitUserId != null,
                        token);

                if (dbUser == null)
                {
                    await command.FollowupChunkAsync("The given user is not in database or is not a duplicity",
                        ephemeral: true, options: new RequestOptions() {CancelToken = token});
                }
                else
                {
                    dbUser.DuplicityApproved = true;

                    await context.SaveChangesAsync(token);
                    await command.FollowupChunkAsync("Duplicity approved. Link for authentication is: **LINK**",
                        ephemeral: true, options: new RequestOptions() {CancelToken = token});
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not get the given user from database or the changes could not be saved");
                await command.FollowupChunkAsync(
                    "Could not get the given user from database or the changes could not be saved",
                    ephemeral: true,
                    options: new RequestOptions() {CancelToken = token});
                throw;
            }
        }

        public async Task HandleShowDuplicity(SocketSlashCommand command, IUser user, CancellationToken token = default)
        {
            List<Embed> embeds = new List<Embed>();
            try
            {
                List<ulong> duplicities = await _identityResolver.GetDuplicitiesDiscordIdsList(user.Id, token);

                foreach (ulong targetUser in duplicities)
                {
                    EmbedBuilder embedBuilder = new EmbedBuilder();

                    IUser? currentUser = await _client.GetUserAsync((ulong) targetUser);
                    if (currentUser == null)
                    {
                        embedBuilder.WithAuthor(new EmbedAuthorBuilder()
                            .WithName($@"Could not find discord mapping - known discord id: {targetUser}"));
                    }
                    else
                    {
                        embedBuilder
                            .WithAuthor(currentUser);
                    }

                    embeds.Add(embedBuilder
                        .WithTitle("Duplicity")
                        .WithDescription("This is a duplicity of the specified user.")
                        .WithFooter(
                            $@"For identity of this user, use /users showidentity discordid:{targetUser}")
                        .Build());
                }

                if (embeds.Count == 0)
                {
                    await command.FollowupChunkAsync("Could not find any duplicit records", ephemeral: true);
                }
                else
                {
                    await command.FollowupChunkAsync(text: "Found records", embeds: embeds.ToArray(), ephemeral: true,
                        options: new RequestOptions() {CancelToken = token});
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not get the users from database.");
                await command.FollowupChunkAsync("Could not get the users from database.", ephemeral: true,
                    options: new RequestOptions() {CancelToken = token});
            }
        }

        public async Task HandleAddUser(SocketSlashCommand command, IUser user, string ctuUsername,
            CancellationToken token = default)
        {
            DbUser dbUser = new DbUser()
            {
                CtuUsername = ctuUsername,
                DiscordId = user.Id
            };
            try
            {
                await using ChristofelBaseContext context = _dbContextFactory.CreateDbContext();

                context.Add(dbUser);
                await context.SaveChangesAsync(token);
                await command.FollowupChunkAsync($@"New user {user.Mention} added. You can assign him roles manually",
                    ephemeral: true);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "There was an error while saving data to the database");
                await command.FollowupChunkAsync("There was an error while saving data to the database",
                    ephemeral: true, options: new RequestOptions() {CancelToken = token});
                throw;
            }
        }

        public async Task HandleShowIdentity(SocketSlashCommand command, IUser? user, string? discordId,
            CancellationToken token = default)
        {
            Verified<UserData> verified = await new CommandVerifier<UserData>(_client, command, _logger)
                .VerifyUserOrUserId(user, discordId)
                .FinishVerificationAsync();

            if (!verified.Success)
            {
                return;
            }

            UserData data = verified.Result;
            if (data.DiscordId == null)
            {
                throw new InvalidOperationException("Verification failed");
            }

            try
            {
                List<string> identities = (await _identityResolver.GetIdentitiesDiscordIdsList((ulong) data.DiscordId))
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

                await command.FollowupChunkAsync(response, ephemeral: true,
                    options: new RequestOptions() {CancelToken = token});

                if (notifyUser)
                {
                    try
                    {
                        IUser? targetUser = await _client.GetUserAsync((ulong) data.DiscordId,
                            new RequestOptions() {CancelToken = token});
                        IDMChannel? dmChannel =
                            await targetUser.CreateDMChannelAsync(new RequestOptions() {CancelToken = token});

                        ILinkUser? commandUserIdentity = await _identityResolver.GetFirstIdentity(command.User.Id);
                        await dmChannel.SendMessageAsync(
                            $@"Ahoj, uživatel {commandUserIdentity?.CtuUsername ?? "(ČVUT údaje nebyly nalezeny)"} alias {command.User.Mention} právě zjišťoval tvůj username. Pokud máš pocit, že došlo ke zneužití, kontaktuj podporu.");
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Could not send DM message to the user notifying him about being identified");
                        await command.FollowupAsync(
                            "Could not send DM message to the user notifying him about being identified", ephemeral: true,
                            options: new RequestOptions(){CancelToken = token});
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not get the user from the database");
                await command.FollowupChunkAsync("Could not get the user from the database", ephemeral: true,
                    options: new RequestOptions() {CancelToken = token});
            }
        }

        private SlashCommandBuilder GetUsersCommandBuilder()
        {
            return new SlashCommandBuilder()
                .WithName("users")
                .WithDescription("Manage users")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("add")
                    .WithDescription("Manually add the user to the database")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("user")
                        .WithDescription("Discord user to add")
                        .WithRequired(true)
                        .WithType(ApplicationCommandOptionType.User))
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("ctuusername")
                        .WithDescription("CTU username of the user")
                        .WithRequired(true)
                        .WithType(ApplicationCommandOptionType.String)))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("showidentity")
                    .WithDescription("Get users identity")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("user")
                        .WithDescription("Discord user to show identity of")
                        .WithType(ApplicationCommandOptionType.User))
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("discordid")
                        .WithDescription("Id of the user in case he is not on this server anymore")
                        .WithType(ApplicationCommandOptionType.String)));
        }

        private SlashCommandBuilder GetDuplicityCommandBuilder()
        {
            return new SlashCommandBuilder()
                .WithName("duplicity")
                .WithDescription("Manage user duplicities")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("show")
                    .WithDescription("Show information about a duplicity")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("user")
                        .WithDescription("User to show information about")
                        .WithRequired(true)
                        .WithType(ApplicationCommandOptionType.User)))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("allow")
                    .WithDescription("Allow a duplicity")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("user")
                        .WithDescription("User to allow duplicity")
                        .WithRequired(true)
                        .WithType(ApplicationCommandOptionType.User)));
        }

        public Task SetupCommandsAsync(ICommandHolder holder, CancellationToken token = new CancellationToken())
        {
            SlashCommandHandler duplicityHandler = new SubCommandHandlerCreator()
                .CreateHandlerForCommand(
                    ("show", (CommandDelegate<IUser>) HandleShowDuplicity),
                    ("allow", (CommandDelegate<IUser>) HandleAllowDuplicity));

            SlashCommandHandler userHandler = new SubCommandHandlerCreator()
                .CreateHandlerForCommand(
                    ("add", (CommandDelegate<IUser, string>) HandleAddUser),
                    ("showidentity", (CommandDelegate<IUser?, string?>) HandleShowIdentity));

            SlashCommandInfoBuilder userBuilder = new SlashCommandInfoBuilder()
                .WithPermission("management.users.manage")
                .WithGuild(_options.GuildId)
                .WithHandler(userHandler)
                .WithBuilder(GetUsersCommandBuilder());

            SlashCommandInfoBuilder duplicityBuilder = new SlashCommandInfoBuilder()
                .WithPermission("management.users.duplicities")
                .WithHandler(duplicityHandler)
                .WithGuild(_options.GuildId)
                .WithBuilder(GetDuplicityCommandBuilder());

            ICommandExecutor executor = new CommandExecutorBuilder()
                .WithLogger(_logger)
                .WithPermissionsCheck(_resolver)
                .WithDeferMessage()
                .WithThreadPool()
                .Build();

            holder.AddCommand(userBuilder, executor);
            holder.AddCommand(duplicityBuilder, executor);
            return Task.CompletedTask;
        }
    }
}