using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Discord;
using Christofel.BaseLib.Permissions;
using Christofel.CommandsLib;
using Christofel.CommandsLib.Commands;
using Christofel.CommandsLib.Extensions;
using Discord;
using Discord.WebSocket;

namespace Christofel.Application.Commands
{
    public class ControlCommands : CommandHandler
    {
        private IReadableConfig _config;
        private IBot _bot;
        private ChristofelApp _app;

        // Quit, Refresh
        public ControlCommands(DiscordSocketClient client, IPermissionService permissions, IReadableConfig config, IBot bot, ChristofelApp app) : base(client, permissions)
        {
            _bot = bot;
            _app = app;
            _config = config;
        }

        public override async Task SetupCommandsAsync()
        {
            SlashCommandBuilder quitBuilder = new SlashCommandBuilderInfo()
                .WithName("quit")
                .WithDescription("Quit the bot")
                .WithPermission("application.quit")
                .WithGuild(await _config.GetAsync<ulong>("discord.bot.guild"))
                .WithHandler(HandleQuitCommand);
            
            SlashCommandBuilder refreshBuilder = new SlashCommandBuilderInfo()
                .WithName("refresh")
                .WithDescription("Refresh config where it can be")
                .WithPermission("application.refresh")
                .WithGuild(await _config.GetAsync<ulong>("discord.bot.guild"))
                .WithHandler(HandleRefreshCommand);

            await RegisterCommandAsync(quitBuilder);
            await RegisterCommandAsync(refreshBuilder);
        }

        private Task HandleRefreshCommand(SocketSlashCommand command)
        {
            return _app.RefreshAsync();
        }

        private Task HandleQuitCommand(SocketSlashCommand command)
        {
            _bot.QuitBot();
            return Task.CompletedTask;
        }
    }
}