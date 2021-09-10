//
//   TitleRoleAssignment.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

namespace Christofel.BaseLib.Database.Models
{
    /// <summary>
    /// Database table for assignment based on titles in their name
    /// </summary>
    public class TitleRoleAssignment
    {
        [Key] public int TitleRoleAssignmentId { get; set; }

        [MaxLength(32)] public string Title { get; set; } = null!;

        public bool Post { get; set; }

        public bool Pre { get; set; }

        public uint Priority { get; set; }

        public int AssignmentId { get; set; }

        public RoleAssignment Assignment { get; set; } = null!;
    }
}