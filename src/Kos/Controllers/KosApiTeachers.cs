using System.Threading;
using System.Threading.Tasks;
using Kos.Atom;
using Kos.Data;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace Kos
{
    public class KosApiTeachers : KosApiController
    {
        public KosApiTeachers(RestClient client, ILogger logger) : base(client, logger)
        {
        }
        
        /// <summary>
        /// Call /teachers/{usernameOrId} and return its resopnse
        /// </summary>
        /// <param name="usernameOrId"></param>
        /// <param name="token"></param>
        /// <returns>Null in case of an error</returns>
        public Task<KosTeacher?> GetTeacherAsync(string usernameOrId, CancellationToken token = default)
        {
            IRestRequest request =
                new RestRequest("students/{usernameOrId}", Method.GET)
                    .AddUrlSegment("usernameOrId", usernameOrId);
            
            return GetResponse(usernameOrId, request, token);
        }

        private async Task<KosTeacher?> GetResponse(string identifier, IRestRequest request, CancellationToken token)
        {
            IRestResponse<KosTeacher?> response = await _client.ExecuteAsync<KosTeacher?>(request, token);
            if (!response.IsSuccessful)
            {
                _logger.LogWarning(response.ErrorException,
                    $"Could not obtain kos teacher information({identifier}): {response.StatusCode} {response.ErrorMessage} {response.Content}");
            }

            return response.Data;
        }
    }
}