//
//   CtuOauthHandler.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IdentityModel.Tokens.Jwt;
using Christofel.Common.User;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;

namespace Christofel.OAuth
{
    /// <summary>
    /// Handler of ctu oauth code exchange and token check.
    /// </summary>
    public class CtuOauthHandler : OauthHandler<CtuOauthOptions>, ICtuTokenApi
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CtuOauthHandler"/> class.
        /// </summary>
        /// <param name="options">The options for the oauth.</param>
        /// <param name="logger">The logger.</param>
        public CtuOauthHandler(IOptionsSnapshot<CtuOauthOptions> options, ILogger<CtuOauthHandler> logger)
            : base(options.Get("CtuFel"))
        {
            _logger = logger;
        }

        /// <inheritdoc cref="ICtuTokenApi"/>
        public ICtuUser GetUser(string accessToken)
        {
            // System.IdentityModel.Tokens.SecurityTokenHandler
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(accessToken);

            var ctuUsername = jwtSecurityToken.Claims.First(x => x.Type == "preferred_username").Value;
            return new CtuUser(ctuUsername);
        }

        private record CtuUser(string CtuUsername) : ICtuUser;
    }
}
