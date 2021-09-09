//
//   ICtuTokenProvider.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Api.Ctu
{
    public interface ICtuTokenProvider
    {
        public string? AccessToken { get; set; }
    }

    public class CtuTokenProvider : ICtuTokenProvider
    {
        public string? AccessToken { get; set; }
    }
}