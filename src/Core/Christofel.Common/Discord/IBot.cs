//
//   IBot.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http;
using Microsoft.Extensions.Http;
using Remora.Discord.Gateway;

namespace Christofel.Common.Discord
{
    /// <summary>
    /// State of Discord bot.
    /// </summary>
    public interface IBot
    {
        /// <summary>
        /// Gets Remora Discord client.
        /// </summary>
        public DiscordGatewayClient Client { get; }

        /// <summary>
        /// Gets http client factory with Remora Discord.
        /// </summary>
        public IHttpClientFactory HttpClientFactory { get; }

        /// <summary>
        /// Gets the options for discord http client.
        /// </summary>
        public HttpClientFactoryOptions DiscordHttpClientOptions { get; }
    }
}