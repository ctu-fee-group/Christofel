using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;

namespace Christofel.Api.Ctu.Apis.UsermapApi
{
    public class Usermap
    {
        private readonly UsermapApiOptions _options;
        private readonly ILogger _logger;
        private readonly Dictionary<string, AuthorizedUsermapApi> _authorizedApis;
        
        public Usermap(IOptionsSnapshot<UsermapApiOptions> options, ILogger<Usermap> logger)
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