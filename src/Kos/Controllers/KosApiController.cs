using System.Collections.Generic;
using Kos.Data;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace Kos
{
    public abstract class KosApiController
    {
        protected readonly RestClient _client;
        protected readonly ILogger _logger;

        internal KosApiController(RestClient client, ILogger logger)
        {
            _logger = logger;
            _client = client;
        }
    }
}