//
//   ICtuTokenProvider.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Api.Ctu
{
    /// <summary>
    /// Provider of ctu token.
    /// </summary>
    public interface ICtuTokenProvider
    {
        /// <summary>
        /// Gets or sets the current access token.
        /// </summary>
        public string? AccessToken { get; set; }
    }

    /// <inheritdoc />
#pragma warning disable SA1402
    public class CtuTokenProvider : ICtuTokenProvider
#pragma warning restore SA1402
    {
        /// <inheritdoc />
        public string? AccessToken { get; set; }
    }
}