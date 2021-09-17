//
//   ICtuUser.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Common.User
{
    /// <summary>
    /// User containing ctu username.
    /// </summary>
    public interface ICtuUser
    {
        /// <summary>
        /// Gets the ctu username of the user.
        /// </summary>
        public string CtuUsername { get; }
    }
}