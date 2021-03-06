//
//   RestRequestExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using RestSharp;

namespace Christofel.Api.Extensions
{
    /// <summary>
    /// Class containing extensions for <see cref="RestRequest"/>.
    /// </summary>
    public static class RestRequestExtensions
    {
        /// <summary>
        /// Add individual parameters from dictionary.
        /// </summary>
        /// <param name="request">The request to add the parameters to.</param>
        /// <param name="parameters">The parameters to add to the request.</param>
        public static void AddParameters(this RestRequest request, Dictionary<string, string> parameters)
        {
            foreach (var parameter in parameters)
            {
                request.AddParameter(parameter.Key, parameter.Value);
            }
        }
    }
}