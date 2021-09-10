//
//   DiscordApiOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Api.Discord
{
    /// <summary>
    /// Options used while calling Discord API
    /// </summary>
    public class DiscordApiOptions
    {
        /// <summary>
        /// What url is the API located at
        /// </summary>
        public string BaseUrl { get; set; } = "https://discord.com/api/v9";
    }
}