//
//   TargetType.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.BaseLib.Database.Models.Enums
{
    /// <summary>
    /// Target type (primarily for permissions), meaning if to get discord user, discord role or everyone.
    /// </summary>
    public enum TargetType
    {
        /// <summary>
        /// Targets specific discord user
        /// </summary>
        User,

        /// <summary>
        /// Targets specific discord role
        /// </summary>
        Role,

        /// <summary>
        /// Targets everyone
        /// </summary>
        Everyone,
    }
}