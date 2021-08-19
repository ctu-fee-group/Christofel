using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.CommandsLib;
using Discord;
using Discord.Net.Interactions.Abstractions;
using Discord.Net.Interactions.Executors;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.HelloWorld
{
    public class PingCommandGroup : IChristofelCommandGroup
    {
        private readonly BotOptions _options;
        private readonly ILogger<PingCommandGroup> _logger;
        private readonly ICommandPermissionsResolver<PermissionSlashInfo> _resolver;

        public PingCommandGroup(IOptions<BotOptions> options, ILogger<PingCommandGroup> logger,
            ICommandPermissionsResolver<PermissionSlashInfo> resolver)
        {
            _resolver = resolver;
            _logger = logger;
            _options = options.Value;
        }

        public Task HandlePing(SocketSlashCommand command, CancellationToken token = new CancellationToken())
        {
            _logger.LogInformation("Handling /ping command");
            return command.RespondAsync("Pong!", options: new RequestOptions {CancelToken = token});
        }

        public Task SetupCommandsAsync(ICommandHolder<PermissionSlashInfo> holder, CancellationToken token = new CancellationToken())
        {
            ICommandExecutor<PermissionSlashInfo> executor = new CommandExecutorBuilder<PermissionSlashInfo>()
                .WithPermissionCheck(_resolver)
                .WithLogger(_logger)
                .Build();

            PermissionSlashInfoBuilder pingBuilder = new PermissionSlashInfoBuilder()
                .WithPermission("helloworld.ping")
                .WithGuild(_options.GuildId)
                .WithHandler(HandlePing)
                .WithBuilder(new SlashCommandBuilder()
                    .WithName("ping")
                    .WithDescription("Ping the bot"));

            holder.AddCommand(pingBuilder.Build(), executor);
            return Task.CompletedTask;
        }
    }
}