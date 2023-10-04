//
//   ICtuTokenApi.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Common.User;

namespace Christofel.OAuth
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
        /// <returns>The retrieved user information.</returns>
        public ICtuUser GetUser(string accessToken);
    }
}