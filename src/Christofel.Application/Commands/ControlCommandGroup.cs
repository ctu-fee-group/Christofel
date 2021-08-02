using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Discord;
using Christofel.BaseLib.Lifetime;
using Christofel.BaseLib.Permissions;
using Christofel.CommandsLib;
using Christofel.CommandsLib.Commands;
using Christofel.CommandsLib.CommandsInfo;
using Christofel.CommandsLib.Executors;
using Christofel.CommandsLib.Extensions;
using Christofel.CommandsLib.Handlers;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
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

        private async Task HandleRefreshCommand(SocketSlashCommand command, CancellationToken token = new CancellationToken())
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
            return command.FollowupAsync("Goodbye", options: new RequestOptions() { CancelToken = token});
        }

        public Task SetupCommandsAsync(ICommandHolder holder, CancellationToken token = new CancellationToken())
        {
            SlashCommandBuilder quitBuilder = new SlashCommandBuilderInfo()
                .WithName("quit")
                .WithDescription("Quit the bot")
                .WithPermission("application.quit")
                .WithGuild(_options.GuildId)
                .WithHandler(HandleQuitCommand);
            
            SlashCommandBuilder refreshBuilder = new SlashCommandBuilderInfo()
                .WithName("refresh")
                .WithDescription("Refresh config where it can be")
                .WithPermission("application.refresh")
                .WithGuild(_options.GuildId)
                .WithHandler(HandleRefreshCommand);

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