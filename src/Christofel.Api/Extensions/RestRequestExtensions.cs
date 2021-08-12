using System.Collections.Generic;
using RestSharp;

namespace Christofel.Api.Extensions
{
    public static class RestRequestExtensions
    {
        public static void AddParameters(this IRestRequest request, Dictionary<string, string> parameters)
        {
            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                request.AddParameter(parameter.Key, parameter.Value);
            }
        }
    }
}