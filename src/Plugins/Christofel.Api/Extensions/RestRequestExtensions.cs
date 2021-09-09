//
//   RestRequestExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using RestSharp;

namespace Christofel.Api.Extensions
{
    public static class RestRequestExtensions
    {
        /// <summary>
        ///     Add individual parameters from dictionary
        /// </summary>
        /// <param name="request"></param>
        /// <param name="parameters"></param>
        public static void AddParameters(this IRestRequest request, Dictionary<string, string> parameters)
        {
            foreach (var parameter in parameters)
            {
                request.AddParameter(parameter.Key, parameter.Value);
            }
        }
    }
}