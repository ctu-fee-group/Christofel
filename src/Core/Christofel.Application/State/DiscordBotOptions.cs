//
//   DiscordBotOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Application.State
{
    /// <summary>
    /// Options for logging Discord bot in
    /// </summary>
    public class DiscordBotOptions
    {
        /// <summary>
        /// Bot token
        /// </summary>
        public string Token { get; set; } = null!;
    }
}