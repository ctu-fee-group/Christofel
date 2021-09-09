//
//   IBot.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http;
using Remora.Discord.Gateway;

namespace Christofel.BaseLib.Discord
{
    public interface IBot
    {
        public DiscordGatewayClient Client { get; }

        //public CacheService Cache { get; }

        public IHttpClientFactory HttpClientFactory { get; }
    }
}