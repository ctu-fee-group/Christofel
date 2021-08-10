using System.Collections;
using System.Collections.Generic;

namespace Christofel.Api.OAuth
{
    public class OauthOptions
    {
        public string? ApplicationId { get; set; }
        
        public string? SecretKey { get; set; }

        public string? TokenEndpoint { get; set; }
        
        public ICollection<string> Scopes { get; set; }
    }
}