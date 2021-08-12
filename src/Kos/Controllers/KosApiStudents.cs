using System;
using System.Threading;
using System.Threading.Tasks;
using Kos.Atom;
using Kos.Data;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace Kos
{
    public class KosApiStudents : KosApiController
    {
        internal KosApiStudents(RestClient client, ILogger logger)
            : base(client, logger)
        {
        }

        public Task<KosStudent?> GetStudentAsync(string studyCodeOrId, CancellationToken token = default)
        {
            IRestRequest request =
                new RestRequest("students/{studyCodeOrId}", Method.GET)
                    .AddUrlSegment("studyCodeOrId", studyCodeOrId);
            
            return GetResponse(studyCodeOrId, request, token);
        }

        private async Task<KosStudent?> GetResponse(string identifier, IRestRequest request, CancellationToken token)
        {
            IRestResponse<KosStudent?> response = await _client.ExecuteAsync<KosStudent?>(request, token);
            if (!response.IsSuccessful)
            {
                _logger.LogWarning(response.ErrorException, $"Could not obtain kos student information({identifier}): {response.StatusCode} {response.ErrorMessage} {response.Content}");
            }
            
            return response.Data;
        }
    }
}