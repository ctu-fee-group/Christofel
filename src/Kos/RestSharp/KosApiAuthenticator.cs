using RestSharp;
using RestSharp.Authenticators;

namespace Kos
{
    /// <summary>
    /// Authenticator using access_token as GET parameter
    /// </summary>
    public class KosApiAuthenticator : AuthenticatorBase
    {
        public KosApiAuthenticator(string token) : base(token)
        {
        }

        protected override Parameter GetAuthenticationParameter(string accessToken) =>
            new Parameter("access_token", accessToken, ParameterType.QueryString);
    }
}