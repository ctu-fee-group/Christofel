//
//   DiscordOauthHandler.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;

namespace Christofel.Api.OAuth
{
    public class DiscordOauthHandler : OauthHandler<IOauthOptions>
    {
        public DiscordOauthHandler(IOptionsSnapshot<OauthOptions> options)
            : base(options.Get("Discord"))
        {
        }
    }
}