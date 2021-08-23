using System.Collections.Generic;

namespace Christofel.Api.OAuth
{
    public class OauthOptions : IOauthOptions
    {
        public string? ApplicationId { get; set; }
        public string? SecretKey { get; set; }
        public string? TokenEndpoint { get; set; }
        public ICollection<string>? Scopes { get; set; }
    }
}