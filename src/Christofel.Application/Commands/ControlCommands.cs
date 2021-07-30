using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Discord;
using Christofel.BaseLib.Permissions;
using Christofel.CommandsLib;
using Christofel.CommandsLib.Commands;
using Christofel.CommandsLib.Extensions;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Application.Commands
{
    public class ControlCommands : CommandHandler
    {
        private IBot _bot;
        private RefreshChristofel _refresh;
        private readonly BotOptions _options;

        // Quit, Refresh
        public ControlCommands(
            DiscordSocketClient client,
            IPermissionService permissions,
            IBot bot,
            RefreshChristofel refresh,
            IOptions<BotOptions> options,
            ILogger<ControlCommands> logger
            ) : base(client, permissions, logger)
        {
            _bot = bot;
            _refresh = refresh;
            _options = options.Value;

            AutoDefer = true;
        }

        public override async Task SetupCommandsAsync(CancellationToken token = new CancellationToken())
        {
            token.ThrowIfCancellationRequested();

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

            await RegisterCommandAsync(quitBuilder, token);
            await RegisterCommandAsync(refreshBuilder, token);
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
            _bot.QuitBot();
            return command.FollowupAsync("Goodbye", options: new RequestOptions() { CancelToken = token});
        }
    }
}