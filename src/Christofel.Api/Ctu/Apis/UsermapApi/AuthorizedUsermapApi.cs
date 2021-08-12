using System;
using System.Net.Cache;
using Microsoft.Extensions.Logging;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers.NewtonsoftJson;

namespace Christofel.Api.Ctu.Apis.UsermapApi
{
    public class AuthorizedUsermapApi
    {
        private readonly RestClient _client;
        private UsermapApiPeople? _people;
        private readonly ILogger _logger;

        public AuthorizedUsermapApi(string accessToken, UsermapApiOptions options, ILogger logger)
        {
            _logger = logger;
            _client = new RestClient(options.BaseUrl ?? throw new InvalidOperationException("BaseUrl is null"))
            {
                CachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable),
                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(accessToken, "Bearer")
            };

            _client.UseNewtonsoftJson();
        }

        public UsermapApiPeople People => _people ??= new UsermapApiPeople(_client, _logger);
    }
}