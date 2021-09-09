//
//   CtuOauthOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Api.OAuth
{
    /// <summary>
    ///     Options for ctu oauth handler
    /// </summary>
    public class CtuOauthOptions : OauthOptions
    {
        /// <summary>
        ///     Endpoint to obtain ctu username at with valid token
        /// </summary>
        public string? CheckTokenEndpoint { get; set; }
    }
}