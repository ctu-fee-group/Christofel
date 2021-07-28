using Microsoft.Extensions.Logging;

namespace Christofel.Application.Logging.Discord
{
    public class DiscordLoggerCofiguration
    {
        public LogLevel MinLevel { get; set; }
        
        public ulong GuildId { get; set; }
        
        public ulong ChannelId { get; set; }
    }
}