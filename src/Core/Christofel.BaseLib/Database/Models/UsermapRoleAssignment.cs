//
//   UsermapRoleAssignment.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

namespace Christofel.BaseLib.Database.Models
{
    /// <summary>
    ///     Database table for assginments using Usermap roles
    /// </summary>
    public class UsermapRoleAssignment
    {
        [Key] public int UsermapRoleAssignmentId { get; set; }

        [MaxLength(512)] public string UsermapRole { get; set; } = null!;

        /// <summary>
        ///     If true, match by regex.
        ///     If false, match the whole string.
        /// </summary>
        public bool RegexMatch { get; set; }

        public int AssignmentId { get; set; }

        public RoleAssignment Assignment { get; set; } = null!;
    }
}