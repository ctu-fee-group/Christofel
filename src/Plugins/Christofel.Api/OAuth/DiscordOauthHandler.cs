using Microsoft.Extensions.Options;

namespace Christofel.Api.OAuth
{
    public class DiscordOauthHandler : OauthHandler<IOauthOptions>
    {
        public DiscordOauthHandler(IOptionsSnapshot<OauthOptions> options)
            : base(options.Get("Discord"))
        {
        }
    }
}