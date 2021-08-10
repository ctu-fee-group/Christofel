using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Christofel.Api.Discord
{
    public class AuthorizedDiscordApi
    {
        private readonly HttpClient _httpClient;
        private readonly string _accessToken;

        internal AuthorizedDiscordApi(string accessToken, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _accessToken = accessToken;
        }

        private void AddAuthHeaders(HttpWebRequest request)
        {
            request.Headers.Add("Authorization", $"Bearer {_accessToken}");
        }

        // GET /users/@me
        public async Task<DiscordUser> GetMe()
        {
            using HttpResponseMessage response = await _httpClient.GetAsync("/users/@me");
            
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Discord request for user wasn't successful {response.StatusCode} {await response.Content.ReadAsStringAsync()}");
            }

            string responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<DiscordUser>(responseContent);
        }
    }
}