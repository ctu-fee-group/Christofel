using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu.Apis.UsermapApi.Data;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace Christofel.Api.Ctu.Apis.UsermapApi
{
    public class UsermapApiPeople
    {
        private readonly RestClient _client;
        private readonly ILogger _logger;
        private readonly Dictionary<string, UsermapPerson?> _cachedPeople;

        public UsermapApiPeople(RestClient client, ILogger logger)
        {
            _cachedPeople = new Dictionary<string, UsermapPerson?>();
            _logger = logger;
            _client = client;
        }

        public async Task<UsermapPerson?> GetPersonAsync(string username, CancellationToken token = default)
        {
            if (_cachedPeople.ContainsKey(username))
            {
                return _cachedPeople[username];
            }
            
            IRestRequest request = new RestRequest("/people/{username}", Method.GET)
                .AddUrlSegment("username", username);

            IRestResponse<UsermapPerson?> response = await _client.ExecuteAsync<UsermapPerson?>(request, token);
            _cachedPeople[username] = response.Data;
            
            if (!response.IsSuccessful || response.Data == null)
            {
                _logger.LogWarning($"Could not obtain usermap user information({username}): {response.StatusCode} {response.ErrorMessage} {response.Content}");
                return null;
            }

            return response.Data;
        }
    }
}