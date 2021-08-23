using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Lifetime;
using Christofel.CommandsLib;
using Discord;
using Discord.Net.Interactions.Abstractions;
using Discord.Net.Interactions.Executors;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Application.Commands
{
    /// <summary>
    /// Handler of /refresh and /quit commands
    /// </summary>
    public class ControlCommands : ICommandGroup
    {
        private readonly IApplicationLifetime _applicationLifetime;
        private readonly RefreshChristofel _refresh;
        private readonly ILogger<ControlCommands> _logger;
        private readonly ICommandPermissionsResolver<PermissionSlashInfo> _resolver;

        public ControlCommands(
            ICommandPermissionsResolver<PermissionSlashInfo> resolver,
            IApplicationLifetime lifetime,
            RefreshChristofel refresh,
            ILogger<ControlCommands> logger
        )
        {
            _resolver = resolver;
            _logger = logger;
            _applicationLifetime = lifetime;
            _refresh = refresh;
        }

        private async Task HandleRefreshCommand(SocketInteraction command,
            CancellationToken token = new CancellationToken())
        {
            _logger.LogInformation("Handling command /refresh");
            await _refresh(token);
            _logger.LogInformation("Refreshed successfully");

            await command.FollowupAsync("Refreshed");
        }

        private Task HandleQuitCommand(SocketInteraction command, CancellationToken token = new CancellationToken())
        {
            _logger.LogInformation("Handling command /quit");
            _applicationLifetime.RequestStop();
            return command.FollowupAsync("Goodbye", options: new RequestOptions() {CancelToken = token});
        }

        public Task SetupCommandsAsync(IInteractionHolder holder, CancellationToken token = new CancellationToken())
        {
            PermissionSlashInfoBuilder quitBuilder = new PermissionSlashInfoBuilder()
                .WithPermission("application.quit")
                .WithHandler((DiscordInteractionHandler)HandleQuitCommand)
                .WithBuilder(new SlashCommandBuilder()
                    .WithName("quit")
                    .WithDescription("Quit the bot")
                );

            PermissionSlashInfoBuilder refreshBuilder = new PermissionSlashInfoBuilder()
                .WithPermission("application.refresh")
                .WithHandler((DiscordInteractionHandler)HandleRefreshCommand)
                .WithBuilder(new SlashCommandBuilder()
                    .WithName("refresh")
                    .WithDescription("Refresh config where it can be")
                );

            IInteractionExecutor executor = new InteractionExecutorBuilder<PermissionSlashInfo>()
                .WithLogger(_logger)
                .WithPermissionCheck(_resolver)
                .WithDeferMessage()
                .WithThreadPool()
                .Build();

            holder.AddInteraction(quitBuilder.Build(), executor);
            holder.AddInteraction(refreshBuilder.Build(), executor);
            return Task.CompletedTask;
        }
    }
}