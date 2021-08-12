using System.Collections.Generic;
using System.Collections.Immutable;
using Newtonsoft.Json;

namespace Christofel.Api.OAuth
{
    public record OauthResponse
    {
        public OauthSuccessResponse? SuccessResponse { get; init; }
        public OauthErrorResponse? ErrorResponse { get; init; }

        public bool IsError => ErrorResponse != null;
    }

    public record OauthSuccessResponse(
        [JsonProperty("access_token")] string AccessToken,
        [JsonProperty("token_type")] string TokenType,
        [JsonProperty("expires_in")] int ExpiresIn,
        [JsonProperty("refresh_token")] string RefreshToken,
        [JsonProperty("scope")] string Scope
    );

    public record OauthErrorResponse([JsonProperty("error")] string Error,
        [JsonProperty("error_description")] string ErrorDescription)
    {
        public string Headers { get; set; }
        public string Body { get; set; }
        public int? StatusCode { get; set; }
    }
}