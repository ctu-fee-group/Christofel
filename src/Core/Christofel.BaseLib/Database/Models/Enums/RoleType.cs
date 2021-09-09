//
//   RoleType.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.BaseLib.Database.Models.Enums
{
    /// <summary>
    ///     Types of roles can be used to specify different behaviors for each of them.
    ///     By default only one of each roles should be assigned.
    /// </summary>
    public enum RoleType
    {
        General,
        Year,
        Programme,
        FinishedStudies,
        CurrentStudies,
        Faculty,
    }
}