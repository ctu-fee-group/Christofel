//
//   OauthOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.OAuth
{
    /// <summary>
    /// The options for <see cref="OauthHandler{TOptions}"/>.
    /// </summary>
    public class OauthOptions : IOauthOptions
    {
        /// <inheritdoc cref="IOauthOptions"/>
        public string? ApplicationId { get; set; }

        /// <inheritdoc cref="IOauthOptions"/>
        public string? SecretKey { get; set; }

        /// <inheritdoc cref="IOauthOptions"/>
        public string? TokenEndpoint { get; set; }

        /// <inheritdoc cref="IOauthOptions"/>
        public ICollection<string>? Scopes { get; set; }
    }
}