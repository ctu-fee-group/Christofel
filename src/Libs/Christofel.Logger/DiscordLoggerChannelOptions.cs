//
//   DiscordLoggerChannelOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Christofel.Logger
{
    /// <summary>
    /// Options for logging into Discord channel.
    /// </summary>
    public class DiscordLoggerChannelOptions
    {
        /// <summary>
        /// Gets or sets what minimal level should be logged to the current channel.
        /// </summary>
        public LogLevel MinLevel { get; set; }

        /// <summary>
        /// Gets or sets where should the message be sent.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// Gets or sets where should the message be sent.
        /// </summary>
        public ulong ChannelId { get; set; }
    }
}