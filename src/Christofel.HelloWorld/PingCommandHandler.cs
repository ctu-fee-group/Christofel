using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Permissions;
using Christofel.CommandsLib;
using Christofel.CommandsLib.Commands;
using Christofel.CommandsLib.Extensions;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.HelloWorld
{
    public class PingCommandHandler : CommandHandler
    {
        private readonly BotOptions _options;
        
        public PingCommandHandler(IOptions<BotOptions> options, DiscordSocketClient client, IPermissionService permissions, ILogger<PingCommandHandler> logger)
            : base(client, permissions, logger)
        {
            _options = options.Value;

            RunMode = RunMode.SameThread;
            AutoDefer = false;
        }

        public override Task SetupCommandsAsync(CancellationToken token = new CancellationToken())
        {
            token.ThrowIfCancellationRequested();

            SlashCommandBuilder pingBuilder = new SlashCommandBuilderInfo()
                .WithName("ping")
                .WithDescription("Ping the bot")
                .WithPermission("helloworld.ping")
                .WithGuild(_options.GuildId)
                .WithHandler(HandlePing);

            return RegisterCommandAsync(pingBuilder, token);
        }

        public Task HandlePing(SocketSlashCommand command, CancellationToken token = new CancellationToken())
        {
            _logger.LogInformation("Handling /ping command");
            return command.RespondAsync("Pong!", options: new RequestOptions { CancelToken = token });
        }
    }
}