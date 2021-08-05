using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Permissions;
using Christofel.CommandsLib.Commands;
using Christofel.CommandsLib.CommandsInfo;
using Christofel.CommandsLib.Executors;
using Christofel.CommandsLib.HandlerCreator;
using Christofel.CommandsLib.Extensions;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Management.Commands
{
    public class UserCommandsGroup : ICommandGroup
    {
        // /users add @user ctuUsername
        // /duplicity allow @user
        //   - respond who is the duplicity
        //   - respond with auth link
        // /duplicity show @user
        //   - show duplicate information
        
        private readonly DiscordSocketClient _client;
        private readonly BotOptions _options;
        private readonly ILogger<MessageCommandsGroup> _logger;
        private readonly IPermissionsResolver _resolver;

        public UserCommandsGroup(IOptions<BotOptions> options, IPermissionsResolver resolver,
            ILogger<MessageCommandsGroup> logger, DiscordSocketClient client)
        {
            _client = client;
            _options = options.Value;
            _logger = logger;
            _resolver = resolver;
        }

        public Task HandleAllowDuplicity(SocketSlashCommand command, IUser user, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task HandleShowDuplicity(SocketSlashCommand command, IUser user, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task HandleAddUser(SocketSlashCommand command, IUser user, string ctuUsername,
            CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task SetupCommandsAsync(ICommandHolder holder, CancellationToken token = new CancellationToken())
        {
            SlashCommandHandler duplicityHandler = new SubCommandHandlerCreator()
                .CreateHandlerForCommand(
                    ("show", (CommandDelegate<IUser>) HandleShowDuplicity),
                    ("allow", (CommandDelegate<IUser>) HandleAllowDuplicity));

            SlashCommandHandler userHandler = new SubCommandHandlerCreator()
                .CreateHandlerForCommand(("add", (CommandDelegate<IUser, string>) HandleAddUser));

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