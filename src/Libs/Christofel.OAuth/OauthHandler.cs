//
//   OauthHandler.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers.Json;
using RestSharp.Serializers.NewtonsoftJson;

namespace Christofel.OAuth
{
    /// <summary>
    /// Handler of post oauth access token retrieval.
    /// </summary>
    /// <typeparam name="TOptions">The type of the ctu options.</typeparam>
    public abstract class OauthHandler<TOptions>
        where TOptions : IOauthOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OauthHandler{TOptions}"/> class.
        /// </summary>
        /// <param name="options">The options of the oauth handler.</param>
        protected OauthHandler(TOptions options)
        {
            Options = options;

            var restOptions = new RestClientOptions();
            restOptions.Authenticator = new HttpBasicAuthenticator
            (
                Options.ApplicationId ?? throw new InvalidOperationException("ApplicationId is null"),
                Options.SecretKey ?? throw new InvalidOperationException("SecretKey is null")
            );

            Client = new RestClient
            (
                options: restOptions,
                configureSerialization: s => s.UseSystemTextJson()
            );

        }

        /// <summary>
        /// Gets the client used for http requests.
        /// </summary>
        protected RestClient Client { get; }

        /// <summary>
        /// Gets the options of the oauth handler.
        /// </summary>
        protected TOptions Options { get; }

        /// <summary>
        /// Grant user authorization code.
        /// </summary>
        /// <remarks>
        /// Used for authorizing a user directly.
        /// </remarks>
        /// <param name="code">The code from the user's oauth.</param>
        /// <param name="redirectUri">The specified redirect uri.</param>
        /// <param name="token">The cancellation token for the operation..</param>
        /// <returns>Response from the oauth, may not be successful.</returns>
        /// <exception cref="InvalidOperationException">Thrown if options token endpoint is missing.</exception>
        public async Task<OauthResponse> GrantAuthorizationCodeAsync
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
                { "scope", string.Join(' ', Options.Scopes ?? Enumerable.Empty<string>()) },
            };

            return await ExchangeTokenAsync(tokenRequestParameters, token);
        }

        /// <summary>
        /// Grant client credentials for service.
        /// </summary>
        /// <remarks>
        /// Used for getting an access token for the service application without a user.
        /// </remarks>
        /// <param name="token">The cancellation token for the operation..</param>
        /// <returns>Response from the oauth, may not be successful.</returns>
        public async Task<OauthResponse> GrantClientCredentialsAsync(CancellationToken token = default)
        {
            var tokenRequestParameters = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "scope", string.Join(' ', Options.Scopes ?? Enumerable.Empty<string>()) },
            };

            return await ExchangeTokenAsync(tokenRequestParameters, token);
        }

        private async Task<OauthResponse> ExchangeTokenAsync
            (Dictionary<string, string> parameters, CancellationToken token)
        {
            var request = new RestRequest
                (Options.TokenEndpoint ?? throw new InvalidOperationException("TokenEndpoint is null"), Method.Post);
            request.AddOrUpdateParameters
            (
                parameters
                    .Select(x => Parameter.CreateParameter(x.Key, x.Value, ParameterType.GetOrPost))
            );

            var response = await Client.ExecuteAsync(request, token);
            return ProcessTokenResponse(response);
        }

        /// <summary>
        /// Processes response from the token endpoint request.
        /// </summary>
        /// <param name="response">The response to process.</param>
        /// <returns>Response of the oauth.</returns>
        private OauthResponse ProcessTokenResponse(RestResponse response)
        {
            OauthSuccessResponse? successResponse = null;
            OauthErrorResponse? errorResponse = null;

            if (response.IsSuccessful)
            {
                successResponse = JsonConvert.DeserializeObject<OauthSuccessResponse>(response.Content ?? string.Empty);
            }
            else
            {
                errorResponse = JsonConvert.DeserializeObject<OauthErrorResponse>(response.Content ?? string.Empty) ??
                                new OauthErrorResponse("Unknown", "Unknown");
                errorResponse.Body = response.Content ?? string.Empty;
                errorResponse.Headers = string.Join("; ", response.Headers ?? Array.Empty<HeaderParameter>());
                errorResponse.StatusCode = (int)response.StatusCode;
            }

            return new OauthResponse
            {
                SuccessResponse = successResponse,
                ErrorResponse = errorResponse
            };
        }
    }
}