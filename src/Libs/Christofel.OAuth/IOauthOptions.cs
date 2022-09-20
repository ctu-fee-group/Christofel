//
//   IOauthOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.OAuth
{
    /// <summary>
    /// Options for oauth handlers with needed data.
    /// </summary>
    public interface IOauthOptions
    {
        /// <summary>
        /// Gets or sets client id.
        /// </summary>
        public string? ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets secret key.
        /// </summary>
        public string? SecretKey { get; set; }

        /// <summary>
        /// Gets or sets endpoint to obtain access token from.
        /// </summary>
        public string? TokenEndpoint { get; set; }

        /// <summary>
        /// Gets or sets scopes that should be requested.
        /// </summary>
        public ICollection<string>? Scopes { get; set; }
    }
}