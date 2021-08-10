using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Christofel.Api.Discord
{
    public class DiscordApi : IDisposable
    {
        private readonly HttpClient _httpClient;

        public DiscordApi(IOptionsSnapshot<DiscordApiOptions> options)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(options.Value.BasePath);
        }

        // GET /users/@me
        public AuthorizedDiscordApi GetAuthorizedApi(string accessToken) =>
            new AuthorizedDiscordApi(accessToken, _httpClient);

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}