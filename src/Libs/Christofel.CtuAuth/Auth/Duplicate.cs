//
//   Duplicate.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Common.User;
using Remora.Rest.Core;

namespace Christofel.CtuAuth.Auth
{
    /// <summary>
    /// Information about a duplicate user.
    /// </summary>
    /// <param name="Type">The type of the duplicate.</param>
    /// <param name="Users">The duplicate users, if any.</param>
    public record Duplicate(DuplicityType Type, IReadOnlyList<DuplicateUser> Users);

    /// <summary>
    /// A record containing all three possible types of duplicates.
    /// </summary>
    /// <param name="DuplicateFound">Whether there is any duplicate.</param>
    /// <param name="Both">The duplicates of type <see cref="DuplicityType.Both"/> are stored here. Null if none exist.</param>
    /// <param name="Ctu">The duplicates of type <see cref="DuplicityType.CtuSide"/> are stored here. Null if none exist.</param>
    /// <param name="Discord">The duplicates of type <see cref="DuplicityType.DiscordSide"/> are stored here. Null if none exist.</param>
    public record CompositedDuplicates(bool DuplicateFound, Duplicate? Both, Duplicate? Ctu, Duplicate? Discord);

    /// <summary>
    /// Information about a duplicate user.
    /// </summary>
    /// <param name="UserId"></param>
    /// <param name="CtuUsername"></param>
    /// <param name="DiscordId"></param>
    public record DuplicateUser(int UserId, string CtuUsername, Snowflake DiscordId) : ILinkUser;

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