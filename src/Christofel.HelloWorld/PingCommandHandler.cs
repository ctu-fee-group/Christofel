using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Permissions;
using Christofel.CommandsLib;
using Christofel.CommandsLib.Commands;
using Christofel.CommandsLib.Extensions;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace Christofel.HelloWorld
{
    public class PingCommandHandler : CommandHandler
    {
        private readonly BotOptions _options;        
        
        public PingCommandHandler(IOptions<BotOptions> options, DiscordSocketClient client, IPermissionService permissions)
            : base(client, permissions)
        {
            _options = options.Value;
        }

        public override Task SetupCommandsAsync()
        {
            SlashCommandBuilder pingBuilder = new SlashCommandBuilderInfo()
                .WithName("ping")
                .WithDescription("Ping the bot")
                .WithPermission("helloworld.ping")
                .WithGuild(_options.GuildId)
                .WithHandler(HandlePing);

            return RegisterCommandAsync(pingBuilder);
        }

        public Task HandlePing(SocketSlashCommand command)
        {
            return command.RespondAsync("Pong!");
        }
    }
}