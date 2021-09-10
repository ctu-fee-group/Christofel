//
//   Duplicate.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.BaseLib.Database.Models;

namespace Christofel.Api.Ctu.Auth
{
    public enum DuplicityType
    {
        /// <summary>
        /// There is no duplicity found
        /// </summary>
        None,

        /// <summary>
        /// Duplicity on ctu side (meaning the same CTU account is already registered)
        /// </summary>
        CtuSide,

        /// <summary>
        /// Duplicity on Discord side (meaning the same Discord account is already registered)
        /// </summary>
        DiscordSide,

        /// <summary>
        /// This account is already registered, just remove the current user and update his roles
        /// </summary>
        Both,
    }

    public record Duplicate(DuplicityType Type, DbUser? User);
}