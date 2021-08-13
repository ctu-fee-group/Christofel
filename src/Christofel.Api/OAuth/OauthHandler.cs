using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Extensions;
using Discord.Net.Queue;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers.NewtonsoftJson;

namespace Christofel.Api.OAuth
{
    /// <summary>
    /// Handler of post oauth access token retrieval
    /// </summary>
    public class OauthHandler
    {
        protected readonly OauthOptions _options;
        protected readonly RestClient _client;

        public OauthHandler(IOptionsSnapshot<OauthOptions> options)
        {
            _client = new RestClient();
            _client.UseNewtonsoftJson();

            _options = GetOptions(options);
            _client.Authenticator = new HttpBasicAuthenticator(
                _options.ApplicationId ?? throw new InvalidOperationException("ApplicationId is null"),
                _options.SecretKey ?? throw new InvalidOperationException("SecretKey is null"));
        }

        /// <summary>
        /// Return correct options of the current oauth handler (can be used to distinguish multiple oauth services)
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        protected virtual OauthOptions GetOptions(IOptionsSnapshot<OauthOptions> options)
        {
            return options.Value;
        }

        /// <summary>
        /// Obtain access token or error
        /// </summary>
        /// <param name="code"></param>
        /// <param name="redirectUri"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual async Task<OauthResponse> ExchangeCodeAsync(string code, string redirectUri,
            CancellationToken token = default)
        {
            var tokenRequestParameters = new Dictionary<string, string>()
            {
                { "redirect_uri", redirectUri },
                { "code", code },
                { "grant_type", "authorization_code" },
                { "scope", string.Join(' ', _options.Scopes ?? Enumerable.Empty<string>()) }
            };

            IRestRequest request = new RestSharp.RestRequest(
                _options.TokenEndpoint ?? throw new InvalidOperationException("TokenEndpoint is null"), Method.POST);
            request.AddParameters(tokenRequestParameters);

            IRestResponse response = await _client.ExecuteAsync(request, token);
            return ProcessTokenResponse(response);
        }

        protected virtual OauthResponse ProcessTokenResponse(IRestResponse response)
        {
            OauthSuccessResponse? successResponse = null;
            OauthErrorResponse? errorResponse = null;

            if (response.IsSuccessful)
            {
                successResponse = JsonConvert.DeserializeObject<OauthSuccessResponse>(response.Content);
            }
            else
            {
                errorResponse = JsonConvert.DeserializeObject<OauthErrorResponse>(response.Content);
                errorResponse.Body = response.Content;
                errorResponse.Headers = string.Join("; ", response.Headers);
                errorResponse.StatusCode = (int)response.StatusCode;
            }

            return new OauthResponse { SuccessResponse = successResponse, ErrorResponse = errorResponse };
        }
    }
}