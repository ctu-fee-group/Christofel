using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Database.Models;
using Christofel.CommandsLib;
using Discord;
using Discord.Net.Interactions.Abstractions;
using Discord.Net.Interactions.Executors;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.HelloWorld
{
    public class PingCommandGroup : ICommandGroup
    {
        private readonly ILogger<PingCommandGroup> _logger;
        private readonly ICommandPermissionsResolver<PermissionSlashInfo> _resolver;

        public PingCommandGroup(ILogger<PingCommandGroup> logger,
            ICommandPermissionsResolver<PermissionSlashInfo> resolver)
        {
            _resolver = resolver;
            _logger = logger;
        }

        public Task HandlePing(SocketInteraction command, CancellationToken token = new CancellationToken())
        {
            _logger.LogInformation("Handling /ping command");
            return command.RespondAsync("Pong!", options: new RequestOptions {CancelToken = token});
        }

        public Task SetupCommandsAsync(IInteractionHolder holder, CancellationToken token = new CancellationToken())
        {
            IInteractionExecutor executor = new InteractionExecutorBuilder<PermissionSlashInfo>()
                .WithPermissionCheck(_resolver)
                .WithLogger(_logger)
                .Build();

            PermissionSlashInfoBuilder pingBuilder = new PermissionSlashInfoBuilder()
                .WithPermission("helloworld.ping")
                .WithHandler((DiscordInteractionHandler)HandlePing)
                .WithBuilder(new SlashCommandBuilder()
                    .WithName("ping")
                    .WithDescription("Ping the bot"));

            holder.AddInteraction(pingBuilder.Build(), executor);
            return Task.CompletedTask;
        }
    }
}