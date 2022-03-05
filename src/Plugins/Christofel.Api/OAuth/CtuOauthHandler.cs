//
//   CtuOauthHandler.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Common.User;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;

namespace Christofel.Api.OAuth
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
            : base(options.Get("Ctu"))
        {
            _logger = logger;
        }

        /// <inheritdoc cref="ICtuTokenApi"/>
        public async Task<ICtuUser> CheckTokenAsync(string accessToken, CancellationToken token = default)
        {
            var request = new RestRequest
            (
                Options.CheckTokenEndpoint ?? throw new InvalidOperationException("CheckTokenEndpoint is null"),
                Method.Post
            );
            request.AddParameter("token", accessToken);

            var response =
                await Client.ExecuteAsync<CheckTokenResponse>(request, token);
            if (!response.IsSuccessful)
            {
                throw new InvalidOperationException
                    ($"Could not obtain user information using check token ({response})");
            }

            return response.Data ?? throw new Exception("Could not parse check token data");
        }

        private class CheckTokenResponse : ICtuUser
        {
            [JsonConstructor]
            public CheckTokenResponse([JsonProperty("user_name")] string ctuUsername)
            {
                CtuUsername = ctuUsername;
            }

            public int UserId { get; } = 0;

            [JsonProperty("user_name")]
            public string CtuUsername { get; }
        }
    }
}