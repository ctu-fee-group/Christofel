using Microsoft.Extensions.Options;

namespace Christofel.Api.Discord
{
    /// <summary>
    /// Discord API giving out authorized api with access tokens
    /// </summary>
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