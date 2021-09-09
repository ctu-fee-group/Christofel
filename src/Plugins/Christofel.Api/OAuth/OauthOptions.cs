//
//   OauthOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Christofel.Api.OAuth
{
    public class OauthOptions : IOauthOptions
    {
        public string? ApplicationId { get; set; }
        public string? SecretKey { get; set; }
        public string? TokenEndpoint { get; set; }
        public ICollection<string>? Scopes { get; set; }
    }
}