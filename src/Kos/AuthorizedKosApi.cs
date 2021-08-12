using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Cache;
using System.Threading;
using System.Threading.Tasks;
using Kos.Atom;
using Kos.Data;
using Microsoft.Extensions.Logging;
using RestSharp;
using RestSharp.Serialization.Xml;

namespace Kos
{
    public class AuthorizedKosApi
    {
        private readonly ILogger _logger;
        private readonly RestClient _client;
        private KosApiPeople? _people;
        private KosApiTeachers? _teachers;
        private KosApiStudents? _students;

        internal AuthorizedKosApi(string accessToken, KosApiOptions options, ILogger logger)
        {
            _logger = logger;
            _client = new RestClient(options.BaseUrl ?? throw new InvalidOperationException("BaseUrl is null"))
            {
                Authenticator = new KosApiAuthenticator(accessToken),
                CachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable)
            };

            _client.UseDotNetXmlSerializer();
        }

        public KosApiPeople People => _people ??= new KosApiPeople(_client, _logger);
        public KosApiTeachers Teachers => _teachers ??= new KosApiTeachers(_client, _logger);
        public KosApiStudents Students => _students ??= new KosApiStudents(_client, _logger);
        
        public async Task<T?> LoadEntityAsync<T>(KosLoadableEntity<T> kosLoadable,
            CancellationToken token = default)
            where T : class, new()
        {
            if (kosLoadable.Href is null)
            {
                _logger.LogWarning($"Cannot obtain href from {typeof(T).FullName} loadable");
                return default;
            }
            
            IRestRequest request =
                new RestRequest(kosLoadable.Href,
                    Method.GET);

            return await GetResponse<T>(kosLoadable.Href, request, token);
        }

        private async Task<T?> GetResponse<T>(string identifier, IRestRequest request, CancellationToken token)
            where T : class, new()
        {
            IRestResponse<AtomEntry<T?>>? response = await _client.ExecuteAsync<AtomEntry<T?>>(request, token);
            if (!response.IsSuccessful || response?.Data == null || response.Data.Content == null)
            {
                _logger.LogWarning(response?.ErrorException, $"Could not obtain kos student information({identifier}): {response?.StatusCode} {response?.ErrorMessage} {response?.Content}");
            }
            
            return response?.Data?.Content;
        }
    }
}