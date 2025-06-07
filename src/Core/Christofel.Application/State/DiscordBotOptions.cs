//
//   DiscordBotOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Application.State
{
    /// <summary>
    /// Options for Discord bot.
    /// </summary>
    public class DiscordBotOptions
    {
        /// <summary>
        /// Gets or sets the token of the bot.
        /// </summary>
        public string Token { get; set; } = null!;
    }
}