//
//   CtuOauthHandler.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.User;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;

namespace Christofel.Api.OAuth
{
    public class CtuOauthHandler : OauthHandler<CtuOauthOptions>, ICtuTokenApi
    {
        private readonly ILogger _logger;

        public CtuOauthHandler(IOptionsSnapshot<CtuOauthOptions> options, ILogger<CtuOauthHandler> logger)
            : base(options.Get("Ctu"))
        {
            _logger = logger;
        }

        /// <summary>
        /// Obtain ctu username from valid access token
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<ICtuUser> CheckTokenAsync(string accessToken, CancellationToken token = default)
        {
            IRestRequest request = new RestRequest
            (
                _options.CheckTokenEndpoint ?? throw new InvalidOperationException("CheckTokenEndpoint is null"),
                Method.POST
            );
            request.AddParameter("token", accessToken);

            IRestResponse<CheckTokenResponse> response =
                await _client.ExecuteAsync<CheckTokenResponse>(request, token);
            if (!response.IsSuccessful)
            {
                throw new InvalidOperationException
                    ($"Could not obtain user information using check token ({response})");
            }

            return response.Data;
        }

        private class CheckTokenResponse : ICtuUser
        {
            [JsonConstructor]
            public CheckTokenResponse([JsonProperty("user_name")] string ctuUsername)
            {
                CtuUsername = ctuUsername;
            }

            public int UserId { get; } = 0;

            [JsonProperty("user_name")] public string CtuUsername { get; }
        }
    }
}