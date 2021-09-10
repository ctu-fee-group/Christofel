//
//   ICtuTokenApi.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.User;

namespace Christofel.Api.OAuth
{
    /// <summary>
    /// Ctu Token api for checking token.
    /// </summary>
    public interface ICtuTokenApi
    {
        /// <summary>
        /// Checks the token using oauth service.
        /// </summary>
        /// <param name="accessToken">The access token to check.</param>
        /// <param name="token">The cancellation token of the operation.</param>
        /// <returns>The retrieved user information.</returns>
        public Task<ICtuUser> CheckTokenAsync(string accessToken, CancellationToken token = default);
    }
}