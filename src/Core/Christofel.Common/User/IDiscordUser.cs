//
//   IDiscordUser.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Rest.Core;

namespace Christofel.Common.User
{
    /// <summary>
    /// Discord user with set DiscordId.
    /// </summary>
    public interface IDiscordUser
    {
        /// <summary>
        /// Gets Discord id of the user.
        /// </summary>
        public Snowflake DiscordId { get; }
    }
}