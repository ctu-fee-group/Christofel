using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Lifetime;
using Christofel.BaseLib.Permissions;
using Christofel.CommandsLib.Commands;
using Christofel.CommandsLib.CommandsInfo;
using Christofel.CommandsLib.Executors;
using Discord;
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
        private readonly BotOptions _options;
        private readonly ILogger<ControlCommands> _logger;
        private readonly IPermissionsResolver _resolver;

        public ControlCommands(
            IPermissionsResolver resolver,
            IApplicationLifetime lifetime,
            RefreshChristofel refresh,
            IOptions<BotOptions> options,
            ILogger<ControlCommands> logger
        )
        {
            _resolver = resolver;
            _logger = logger;
            _applicationLifetime = lifetime;
            _refresh = refresh;
            _options = options.Value;
        }

        private async Task HandleRefreshCommand(SocketSlashCommand command,
            CancellationToken token = new CancellationToken())
        {
            _logger.LogInformation("Handling command /refresh");
            await _refresh(token);
            _logger.LogInformation("Refreshed successfully");

            await command.FollowupAsync("Refreshed");
        }

        private Task HandleQuitCommand(SocketSlashCommand command, CancellationToken token = new CancellationToken())
        {
            _logger.LogInformation("Handling command /quit");
            _applicationLifetime.RequestStop();
            return command.FollowupAsync("Goodbye", options: new RequestOptions() {CancelToken = token});
        }

        public Task SetupCommandsAsync(ICommandHolder holder, CancellationToken token = new CancellationToken())
        {
            SlashCommandInfoBuilder quitBuilder = new SlashCommandInfoBuilder()
                .WithPermission("application.quit")
                .WithGuild(_options.GuildId)
                .WithHandler(HandleQuitCommand)
                .WithBuilder(new SlashCommandBuilder()
                    .WithName("quit")
                    .WithDescription("Quit the bot")
                );

            SlashCommandInfoBuilder refreshBuilder = new SlashCommandInfoBuilder()
                .WithPermission("application.refresh")
                .WithGuild(_options.GuildId)
                .WithHandler(HandleRefreshCommand)
                .WithBuilder(new SlashCommandBuilder()
                    .WithName("refresh")
                    .WithDescription("Refresh config where it can be")
                );

            ICommandExecutor executor = new CommandExecutorBuilder()
                .WithLogger(_logger)
                .WithPermissionsCheck(_resolver)
                .WithDeferMessage()
                .WithThreadPool()
                .Build();

            holder.AddCommand(quitBuilder, executor);
            holder.AddCommand(refreshBuilder, executor);
            return Task.CompletedTask;
        }
    }
}