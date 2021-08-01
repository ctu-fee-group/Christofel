using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Christofel.Application.Logging.Discord
{
    public class DiscordLoggerChannelOptions
    {
        /// <summary>
        /// What minimal level should be logged to the current channel
        /// </summary>
        public LogLevel MinLevel { get; set; }
        
        /// <summary>
        /// Where should the message be sent
        /// </summary>
        public ulong GuildId { get; set; }
        
        /// <summary>
        /// Where should the message be sent
        /// </summary>
        public ulong ChannelId { get; set; }
    }
    
    public class DiscordLoggerOptions
    {
        /// <summary>
        /// Queue size of ConcurrentCollection used in processor of discord messages
        /// </summary>
        public uint MaxQueueSize { get; set; } = 1024;
        
        /// <summary>
        /// What channels to log to
        /// </summary>
        public DiscordLoggerChannelOptions[] Channels { get; set; } = null!;
    }
}