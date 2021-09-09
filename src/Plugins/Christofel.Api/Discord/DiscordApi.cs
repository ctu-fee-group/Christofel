//
//   DiscordApi.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;

namespace Christofel.Api.Discord
{
    /// <summary>
    ///     Discord API giving out authorized api with access tokens
    /// </summary>
    public class DiscordApi
    {
        private readonly DiscordApiOptions _options;

        public DiscordApi(IOptionsSnapshot<DiscordApiOptions> options)
        {
            _options = options.Value;
        }

        // GET /users/@me
        public AuthorizedDiscordApi GetAuthorizedApi(string accessToken) =>
            new AuthorizedDiscordApi(accessToken, _options);
    }
}