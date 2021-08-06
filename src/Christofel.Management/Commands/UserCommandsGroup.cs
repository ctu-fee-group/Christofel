using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Permissions;
using Christofel.CommandsLib.Commands;
using Christofel.CommandsLib.CommandsInfo;
using Christofel.CommandsLib.Executors;
using Christofel.CommandsLib.HandlerCreator;
using Christofel.CommandsLib.Extensions;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Management.Commands
{
    public class UserCommandsGroup : ICommandGroup
    {
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

        public UserCommandsGroup(IOptions<BotOptions> options, IPermissionsResolver resolver,
            ILogger<MessageCommandsGroup> logger, DiscordSocketClient client,
            IDbContextFactory<ChristofelBaseContext> dbContextFactory)
        {
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
                    await command.RespondChunkAsync("The given user is not in database or is not a duplicity",
                        ephemeral: true, options: new RequestOptions() {CancelToken = token});
                }
                else
                {
                    dbUser.DuplicityApproved = true;

                    await context.SaveChangesAsync(token);
                    await command.RespondChunkAsync("Duplicity approved. Link for authentication is: **LINK**",
                        ephemeral: true, options: new RequestOptions() {CancelToken = token});
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not get the given user from database or the changes could not be saved");
                await command.RespondChunkAsync(
                    "Could not get the given user from database or the changes could not be saved", ephemeral: true);
                throw;
            }
        }

        public async Task HandleShowDuplicity(SocketSlashCommand command, IUser user, CancellationToken token = default)
        {
            List<Embed> embeds = new List<Embed>();
            try
            {
                await using ChristofelBaseContext context = _dbContextFactory.CreateDbContext();

                var duplicities = context.Users
                    .AsAsyncEnumerable()
                    .Where(x => x.DiscordId == user.Id ||
                                (x.DuplicitUser != null && x.DuplicitUser.DiscordId == user.Id))
                    .Select(x => new {x.DiscordId, DuplicitUserDiscordId = x.DuplicitUser?.DiscordId})
                    .WithCancellation(token);

                // TODO: move foreach to another method
                await foreach (var duplicity in duplicities)
                {
                    ulong? targetUser = duplicity.DiscordId == user.Id
                        ? duplicity.DuplicitUserDiscordId
                        : duplicity.DiscordId;

                    if (targetUser == null)
                    {
                        continue;
                    }

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
                    await command.RespondChunkAsync("Could not find any duplicit records", ephemeral: true);
                }
                else
                {
                    await command.RespondChunkAsync(text: "Found records", embeds: embeds.ToArray(), ephemeral: true,
                        options: new RequestOptions() {CancelToken = token});
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not get the users from database.");
                await command.RespondChunkAsync("Could not get the users from database.");
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
                await command.RespondChunkAsync($@"New user {user.Mention} added. You can assign him roles manually",
                    ephemeral: true);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "There was an error while saving data to the database");
                await command.RespondChunkAsync("There was an error while saving data to the database",
                    ephemeral: true, options: new RequestOptions() {CancelToken = token});
                throw;
            }
        }

        public async Task HandleShowIdentity(SocketSlashCommand command, IUser? user, string? discordId,
            CancellationToken token = default)
        {
            ulong? targetId = null;

            // TODO: move this kind of validation into CommandVerifier?
            if (user != null)
            {
                targetId = user.Id;
            }
            else if (discordId != null && ulong.TryParse(discordId, out ulong id))
            {
                targetId = id;
            }

            if (targetId == null)
            {
                await command.RespondChunkAsync("User or DiscordId have to be set", ephemeral: true,
                    options: new RequestOptions() {CancelToken = token});
                return;
            }

            // TODO: move to custom method, notify the user about being identified
            try
            {
                await using ChristofelBaseContext context = _dbContextFactory.CreateDbContext();

                List<string> identities = await context.Users
                    .AsNoTracking()
                    .AsQueryable()
                    .Where(x => targetId == x.DiscordId && x.AuthenticatedAt != null)
                    .Select(x => $@"CTU username: {x.CtuUsername}")
                    .ToListAsync(token);

                string response;
                bool notifyUser = true;
                if (identities.Count == 0)
                {
                    notifyUser = false;
                    response =
                        "Could not find this user in database (he may not be authenticated or is not in database)";
                }
                else if (identities.Count == 1)
                {
                    response = "Found exactly one identity for this user: ";
                }
                else
                {
                    response = "Found multiple identities for this user: ";
                }

                response += string.Join(", ", identities);

                await command.RespondChunkAsync(response, ephemeral: true,
                    options: new RequestOptions() {CancelToken = token});

                if (notifyUser)
                {
                    IDMChannel? dmChannel = await _client.GetDMChannelAsync((ulong) targetId);

                    // TODO: get identity of command.User
                    await dmChannel.SendMessageAsync(
                        $@"Ahoj, uživatel TODO alias {command.User.Mention} právě zjišťoval tvůj username. Pokud máš pocit, že došlo ke zneužití, kontaktuj podporu.");
                    await dmChannel.CloseAsync();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not get the user from the database");
                await command.RespondChunkAsync("Could not get the user from the database", ephemeral: true,
                    options: new RequestOptions() {CancelToken = token});
            }
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
                    ("showidentity", (CommandDelegate<IUser, string>) HandleAddUser));

            SlashCommandInfoBuilder userBuilder = new SlashCommandInfoBuilder()
                .WithPermission("management.users.manage")
                .WithGuild(_options.GuildId)
                .WithHandler(userHandler)
                .WithBuilder(new SlashCommandBuilder()
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
                            .WithType(ApplicationCommandOptionType.String))));

            SlashCommandInfoBuilder duplicityBuilder = new SlashCommandInfoBuilder()
                .WithPermission("management.users.duplicities")
                .WithHandler(duplicityHandler)
                .WithGuild(_options.GuildId)
                .WithBuilder(new SlashCommandBuilder()
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
                            .WithType(ApplicationCommandOptionType.User))));

            ICommandExecutor executor = new CommandExecutorBuilder()
                .WithLogger(_logger)
                .WithPermissionsCheck(_resolver)
                .WithThreadPool()
                .Build();

            holder.AddCommand(userBuilder, executor);
            holder.AddCommand(duplicityBuilder, executor);
            return Task.CompletedTask;
        }
    }
}