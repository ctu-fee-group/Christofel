//
//   DiscordOauthHandler.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;

namespace Christofel.Api.OAuth
{
    /// <summary>
    /// Handler of discord oauth code exchange.
    /// </summary>
    public class DiscordOauthHandler : OauthHandler<IOauthOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordOauthHandler"/> class.
        /// </summary>
        /// <param name="options">The options of the oauth.</param>
        public DiscordOauthHandler(IOptionsSnapshot<OauthOptions> options)
            : base(options.Get("Discord"))
        {
        }
    }
}