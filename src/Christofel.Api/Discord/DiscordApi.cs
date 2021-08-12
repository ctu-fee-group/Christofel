using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Christofel.Api.Discord
{
    public class DiscordApi
    {
        private DiscordApiOptions _options;
        
        public DiscordApi(IOptionsSnapshot<DiscordApiOptions> options)
        {
            _options = options.Value;
        }

        // GET /users/@me
        public AuthorizedDiscordApi GetAuthorizedApi(string accessToken) =>
            new AuthorizedDiscordApi(accessToken, _options);
    }
}