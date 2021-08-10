using Microsoft.Extensions.Options;

namespace Christofel.Api.OAuth
{
    public class DiscordOauthHandler : OauthHandler
    {
        public DiscordOauthHandler(IOptionsSnapshot<OauthOptions> options) : base(options)
        {
        }

        protected override OauthOptions GetOptions(IOptionsSnapshot<OauthOptions> options)
        {
            return options.Get("Discord");
        }
    }
}