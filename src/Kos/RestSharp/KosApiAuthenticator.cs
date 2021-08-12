using RestSharp;
using RestSharp.Authenticators;

namespace Kos
{
    public class KosApiAuthenticator : AuthenticatorBase
    {
        public KosApiAuthenticator(string token) : base(token)
        {
        }

        protected override Parameter GetAuthenticationParameter(string accessToken) =>
            new Parameter("access_token", accessToken, ParameterType.QueryString);
    }
}