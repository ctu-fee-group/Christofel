//
//   OauthResponse.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace Christofel.Api.OAuth
{
    /// <summary>
    /// Response from oauth handler token retrieval
    /// </summary>
    public record OauthResponse
    {
        /// <summary>
        /// Set in case of success retrieval
        /// </summary>
        public OauthSuccessResponse? SuccessResponse { get; init; }

        /// <summary>
        /// Set if there was any kind of error
        /// </summary>
        public OauthErrorResponse? ErrorResponse { get; init; }

        public bool IsError => ErrorResponse != null;
    }

    /// <summary>
    /// Holds token information
    /// </summary>
    /// <param name="AccessToken">Token to be used with the service</param>
    /// <param name="TokenType"></param>
    /// <param name="ExpiresIn">Number of seconds till expiration of the token</param>
    /// <param name="RefreshToken">Token used to refresh access token</param>
    /// <param name="Scope">What scopes are allowed</param>
    public record OauthSuccessResponse
    (
        [JsonProperty("access_token")] string AccessToken,
        [JsonProperty("token_type")] string TokenType,
        [JsonProperty("expires_in")] int ExpiresIn,
        [JsonProperty("refresh_token")] string RefreshToken,
        [JsonProperty("scope")] string Scope
    );

    /// <summary>
    /// Holds errors of oauth handling
    /// </summary>
    /// <param name="Error"></param>
    /// <param name="ErrorDescription"></param>
    public record OauthErrorResponse
    (
        [JsonProperty("error")] string Error,
        [JsonProperty("error_description")] string ErrorDescription
    )
    {
        /// <summary>
        /// Response headers
        /// </summary>
        public string Headers { get; set; }

        /// <summary>
        /// Response body
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Response status code
        /// </summary>
        public int? StatusCode { get; set; }
    }
}