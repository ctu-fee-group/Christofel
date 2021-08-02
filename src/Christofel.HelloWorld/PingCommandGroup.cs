using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Permissions;
using Christofel.CommandsLib;
using Christofel.CommandsLib.Commands;
using Christofel.CommandsLib.CommandsInfo;
using Christofel.CommandsLib.Executors;
using Christofel.CommandsLib.Extensions;
using Christofel.CommandsLib.HandlerCreator;
using Christofel.CommandsLib.Handlers;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.HelloWorld
{
    public class PingCommandGroup : ICommandGroup
    {
        private readonly BotOptions _options;
        private readonly ILogger<PingCommandGroup> _logger;
        private readonly IPermissionsResolver _resolver;
        
        public PingCommandGroup(IOptions<BotOptions> options, ILogger<PingCommandGroup> logger, IPermissionsResolver resolver)
        {
            _resolver = resolver;
            _logger = logger;
            _options = options.Value;
        }

        public Task HandlePing(SocketSlashCommand command, CancellationToken token = new CancellationToken())
        {
            _logger.LogInformation("Handling /ping command");
            return command.RespondAsync("Pong!", options: new RequestOptions { CancelToken = token });
        }

        public Task SetupCommandsAsync(ICommandHolder holder, CancellationToken token = new CancellationToken())
        {
            ICommandExecutor executor = new CommandExecutorBuilder()
                .WithPermissionsCheck(_resolver)
                .WithLogger(_logger)
                .Build();

            SlashCommandBuilder pingBuilder = new SlashCommandBuilderInfo()
                .WithName("ping")
                .WithDescription("Ping the bot")
                .WithPermission("helloworld.ping")
                .WithGuild(_options.GuildId)
                .WithHandler(HandlePing);

            holder.AddCommand(pingBuilder, executor);
            return Task.CompletedTask;
        }
    }
}