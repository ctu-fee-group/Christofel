//
//   DiscordLoggerOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Christofel.Logger
{
    /// <summary>
    /// Options for <see cref="DiscordLoggerProvider"/>.
    /// </summary>
    public class DiscordLoggerOptions
    {
        /// <summary>
        /// Gets or sets queue size of ConcurrentCollection used in processor of discord messages.
        /// </summary>
        public uint MaxQueueSize { get; set; } = 1024;

        /// <summary>
        /// Gets or sets maximal count of messages that will be grouped into one Discord message.
        /// </summary>
        public uint MaxGroupSize { get; set; } = 20;

        /// <summary>
        /// Gets or sets what channels to log to.
        /// </summary>
        public DiscordLoggerChannelOptions[] Channels { get; set; } = null!;
    }
}