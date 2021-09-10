//
//   BotOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.BaseLib.Configuration
{
    /// <summary>
    /// Default options for the bot
    /// </summary>
    public class BotOptions
    {
        /// <summary>
        /// This is the main guild id Christofel is part of
        /// </summary>
        public ulong GuildId { get; set; }
    }
}