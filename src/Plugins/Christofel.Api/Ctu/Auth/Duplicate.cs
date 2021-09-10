//
//   Duplicate.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.BaseLib.Database.Models;

namespace Christofel.Api.Ctu.Auth
{
    /// <summary>
    /// Information about a duplicate user.
    /// </summary>
    /// <param name="Type">The type of the duplicate.</param>
    /// <param name="User">The duplicate user, if any.</param>
    public record Duplicate(DuplicityType Type, DbUser? User);

    /// <summary>
    /// Type of the duplicate.
    /// </summary>
    public enum DuplicityType
    {
        /// <summary>
        /// There is no duplicity found.
        /// </summary>
        None,

        /// <summary>
        /// Duplicity on ctu side.
        /// </summary>
        /// <remarks>
        /// The same CTU account is already registered.
        /// </remarks>
        CtuSide,

        /// <summary>
        /// Duplicity on Discord side.
        /// </summary>
        /// <remarks>
        /// The same discord account is already registered.
        /// </remarks>
        DiscordSide,

        /// <summary>
        /// This account is already registered.
        /// </summary>
        /// <remarks>
        /// The new user should be removed from the database and the old one should be updated.
        /// </remarks>
        Both,
    }
}