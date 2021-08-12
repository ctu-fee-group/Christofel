using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Usermap
{
    public class UsermapApi
    {
        private readonly UsermapApiOptions _options;
        private readonly ILogger _logger;
        private readonly Dictionary<string, AuthorizedUsermapApi> _authorizedApis;
        
        public UsermapApi(IOptionsSnapshot<UsermapApiOptions> options, ILogger<UsermapApi> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public AuthorizedUsermapApi GetAuthorizedApi(string accessToken)
        {
            if (_authorizedApis.ContainsKey(accessToken))
            {
                return _authorizedApis[accessToken];
            }
            
            AuthorizedUsermapApi api = new AuthorizedUsermapApi(accessToken, _options, _logger);
            _authorizedApis.Add(accessToken, api);

            return api;
        }
    }
}