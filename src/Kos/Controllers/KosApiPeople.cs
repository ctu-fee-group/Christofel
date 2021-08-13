using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kos.Atom;
using Kos.Data;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace Kos
{
    public class KosApiPeople : KosApiController
    {
        private readonly Dictionary<string, KosPerson?> _cachedPeople;

        internal KosApiPeople(RestClient client, ILogger logger)
            : base(client, logger)
        {
            _cachedPeople = new Dictionary<string, KosPerson?>();
        }

        /// <summary>
        /// Call /people/{username} and return its response
        /// </summary>
        /// <param name="username"></param>
        /// <param name="token"></param>
        /// <returns>Null in case of an error</returns>
        public async Task<KosPerson?> GetPerson(string username, CancellationToken token = default)
        {
            if (_cachedPeople.ContainsKey(username))
            {
                return _cachedPeople[username];
            }

            IRestRequest request = new RestRequest("/people/{username}", Method.GET)
                .AddUrlSegment("username", username);

            IRestResponse<AtomEntry<KosPerson>?> response =
                await _client.ExecuteAsync<AtomEntry<KosPerson>?>(request, token);
            _cachedPeople[username] = response.Data?.Content;

            if (!response.IsSuccessful || response.Data == null || response.Data.Content == null)
            {
                _logger.LogWarning(
                    response.ErrorException,
                    $"Could not obtain kos user information({username}): {response.StatusCode} {response.ErrorMessage} {response.Content}");
                return null;
            }

            return response.Data.Content;
        }
    }
}