//
//   IOauthOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Christofel.Api.OAuth
{
    /// <summary>
    /// Options for oauth handlers with needed data
    /// </summary>
    public interface IOauthOptions
    {
        /// <summary>
        /// Client id
        /// </summary>
        public string? ApplicationId { get; set; }

        /// <summary>
        /// Secret key
        /// </summary>
        public string? SecretKey { get; set; }

        /// <summary>
        /// Endpoint to obtain access token from
        /// </summary>
        public string? TokenEndpoint { get; set; }

        /// <summary>
        /// What scopes should be requested
        /// </summary>
        public ICollection<string>? Scopes { get; set; }
    }
}