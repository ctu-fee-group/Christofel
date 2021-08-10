using Microsoft.Extensions.Options;

namespace Christofel.Api.OAuth
{
    public class CtuOauthHandler : OauthHandler
    {
        public CtuOauthHandler(IOptionsSnapshot<OauthOptions> options) : base(options)
        {
        }

        protected override OauthOptions GetOptions(IOptionsSnapshot<OauthOptions> options)
        {
            return options.Get("Ctu");
        }
    }
}