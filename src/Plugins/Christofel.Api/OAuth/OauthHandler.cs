//
//   OauthHandler.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Extensions;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers.NewtonsoftJson;

namespace Christofel.Api.OAuth
{
    /// <summary>
    ///     Handler of post oauth access token retrieval
    /// </summary>
    public abstract class OauthHandler<TOptions>
        where TOptions : IOauthOptions
    {
        protected readonly RestClient _client;
        protected readonly TOptions _options;

        protected OauthHandler(TOptions options)
        {
            _client = new RestClient();
            _client.UseNewtonsoftJson();

            _options = options;
            _client.Authenticator = new HttpBasicAuthenticator
            (
                _options.ApplicationId ?? throw new InvalidOperationException("ApplicationId is null"),
                _options.SecretKey ?? throw new InvalidOperationException("SecretKey is null")
            );
        }

        /// <summary>
        ///     Obtain access token or error
        /// </summary>
        /// <param name="code"></param>
        /// <param name="redirectUri"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual async Task<OauthResponse> ExchangeCodeAsync
        (
            string code,
            string redirectUri,
            CancellationToken token = default
        )
        {
            var tokenRequestParameters = new Dictionary<string, string>
            {
                { "redirect_uri", redirectUri },
                { "code", code },
                { "grant_type", "authorization_code" },
                { "scope", string.Join(' ', _options.Scopes ?? Enumerable.Empty<string>()) },
            };

            IRestRequest request = new RestRequest
                (_options.TokenEndpoint ?? throw new InvalidOperationException("TokenEndpoint is null"), Method.POST);
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
                errorResponse = JsonConvert.DeserializeObject<OauthErrorResponse>
                    (response.Content) ?? new OauthErrorResponse("Unknown", "Unknown");
                errorResponse.Body = response.Content;
                errorResponse.Headers = string.Join("; ", response.Headers);
                errorResponse.StatusCode = (int) response.StatusCode;
            }

            return new OauthResponse { SuccessResponse = successResponse, ErrorResponse = errorResponse };
        }
    }
}