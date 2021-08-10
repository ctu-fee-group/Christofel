using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Christofel.Api.OAuth
{
    public class OauthHandler
    {
        private readonly OauthOptions _options;
        private readonly HttpClient _client;

        public OauthHandler(IOptionsSnapshot<OauthOptions> options)
        {
            _client = new HttpClient();
            _options = GetOptions(options);
        }

        protected virtual OauthOptions GetOptions(IOptionsSnapshot<OauthOptions> options)
        {
            return options.Value;
        }

        public virtual async Task<OauthResponse> ExchangeCodeAsync(string code, string redirectUri,
            CancellationToken token = default)
        {
            var tokenRequestParameters = new Dictionary<string, string?>()
            {
                { "client_id", _options.ApplicationId },
                { "redirect_uri", redirectUri },
                { "client_secret", _options.SecretKey },
                { "code", code },
                { "grant_type", "authorization_code" },
            };

            FormUrlEncodedContent requestContent = new FormUrlEncodedContent(tokenRequestParameters);
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, _options.TokenEndpoint);
            
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            requestMessage.Content = requestContent;
            
            HttpResponseMessage response = await _client.SendAsync(requestMessage, token);

            return await ProcessTokenResponseAsync(response, token);
        }

        protected virtual async Task<OauthResponse> ProcessTokenResponseAsync(HttpResponseMessage response, CancellationToken token = default)
        {
            OauthSuccessResponse? successResponse = null;
            OauthErrorResponse? errorResponse = null;
            
            if (response.IsSuccessStatusCode)
            {
                successResponse = JsonConvert.DeserializeObject<OauthSuccessResponse>(
                    await response.Content.ReadAsStringAsync(token)
                );
            }
            else
            {
                errorResponse = JsonConvert.DeserializeObject<OauthErrorResponse>(
                    await response.Content.ReadAsStringAsync(token)
                );
                errorResponse.Headers =
                    response.Headers
                        .ToImmutableDictionary(
                            x => x.Key,
                            x => x.Value.ToImmutableArray());
                errorResponse.StatusCode = (int)response.StatusCode;
            }

            return new OauthResponse { SuccessResponse = successResponse, ErrorResponse = errorResponse };
        }
    }
}