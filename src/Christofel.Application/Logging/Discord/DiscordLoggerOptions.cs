using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Christofel.Application.Logging.Discord
{
    public class DiscordLoggerChannelOptions
    {
        public LogLevel MinLevel { get; set; }
        
        public ulong GuildId { get; set; }
        
        public ulong ChannelId { get; set; }
    }
    
    public class DiscordLoggerOptions
    {
        public DiscordLoggerChannelOptions[] Channels { get; set; } = null!;
    }
}