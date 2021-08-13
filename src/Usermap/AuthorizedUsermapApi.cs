using System;
using System.Net.Cache;
using Microsoft.Extensions.Logging;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers.NewtonsoftJson;

namespace Usermap
{
    /// <summary>
    /// Entity used to interact with usermap API
    /// </summary>
    public class AuthorizedUsermapApi
    {
        private readonly RestClient _client;
        private UsermapApiPeople? _people;
        private readonly ILogger _logger;

        internal AuthorizedUsermapApi(string accessToken, UsermapApiOptions options, ILogger logger)
        {
            _logger = logger;
            _client = new RestClient(options.BaseUrl ?? throw new InvalidOperationException("BaseUrl is null"))
            {
                CachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable),
                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(accessToken, "Bearer")
            };

            _client.UseNewtonsoftJson();
        }

        /// <summary>
        /// Endpoint /people
        /// </summary>
        public UsermapApiPeople People => _people ??= new UsermapApiPeople(_client, _logger);
    }
}