using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers.NewtonsoftJson;

namespace Christofel.Api.Discord
{
    /// <summary>
    /// Discord API with assigned accessToken
    /// </summary>
    public class AuthorizedDiscordApi
    {
        private readonly RestClient _client;
        private readonly string _accessToken;

        internal AuthorizedDiscordApi(string accessToken, DiscordApiOptions options)
        {
            _client = new RestClient(options.BaseUrl)
            {
                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(accessToken, "Bearer")
            };
            _client.UseNewtonsoftJson();
            
            _accessToken = accessToken;
        }

        private void AddAuthHeaders(HttpWebRequest request)
        {
            request.Headers.Add("Authorization", $"Bearer {_accessToken}");
        }

        // GET /users/@me
        public async Task<DiscordUser> GetMe()
        {
            IRestRequest request = new RestRequest("/users/@me", Method.GET);
            IRestResponse<DiscordUser> response = await _client.ExecuteAsync<DiscordUser>(request);
            if (!response.IsSuccessful)
            {
                throw new Exception(
                    $"Discord request for user wasn't successful {response.StatusCode} {response.ErrorMessage} {response.Content}",
                    response.ErrorException);
            }

            return response.Data;
        }
    }
}