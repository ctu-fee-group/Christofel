//
//   DiscordApi.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;

namespace Christofel.Api.Discord
{
    /// <summary>
    /// Discord API giving out authorized api with access tokens.
    /// </summary>
    public class DiscordApi
    {
        private readonly DiscordApiOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordApi"/> class.
        /// </summary>
        /// <param name="options">The options for the discord api.</param>
        public DiscordApi(IOptionsSnapshot<DiscordApiOptions> options)
        {
            _options = options.Value;
        }

        /// <summary>
        /// Creates an authorized api with the given token.
        /// </summary>
        /// <param name="accessToken">The token to be used.</param>
        /// <returns>Authorized api that will use the specified access token.</returns>
        public AuthorizedDiscordApi GetAuthorizedApi(string accessToken) =>
            new AuthorizedDiscordApi(accessToken, _options);
    }
}