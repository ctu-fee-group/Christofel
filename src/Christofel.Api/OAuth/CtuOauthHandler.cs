using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Extensions;
using Christofel.BaseLib.User;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;

namespace Christofel.Api.OAuth
{
    public class CtuOauthHandler : OauthHandler
    {
        private class CheckTokenResponse : ICtuUser
        {
            public int UserId { get; } = 0;

            [JsonProperty("user_name")] public string CtuUsername { get; }
        }

        private readonly CtuOauthOptions _options;
        private readonly ILogger _logger;

        public CtuOauthHandler(IOptionsSnapshot<CtuOauthOptions> options, ILogger<CtuOauthHandler> logger)
            : base(options)
        {
            _logger = logger;
            _options = options.Get("Ctu");
        }

        protected override OauthOptions GetOptions(IOptionsSnapshot<OauthOptions> options)
        {
            return options.Get("Ctu");
        }

        public async Task<ICtuUser> CheckTokenAsync(string accessToken, CancellationToken token = default)
        {
            IRestRequest request = new RestSharp.RestRequest(
                _options.CheckTokenEndpoint ?? throw new InvalidOperationException("CheckTokenEndpoint is null"),
                Method.POST);
            request.AddParameter("token", accessToken);

            IRestResponse<CheckTokenResponse> response =
                await _client.ExecuteAsync<CheckTokenResponse>(request, token);
            if (!response.IsSuccessful)
            {
                throw new InvalidOperationException(
                    $"Could not obtain user information using check token ({response})");
            }

            return response.Data;
        }
    }
}