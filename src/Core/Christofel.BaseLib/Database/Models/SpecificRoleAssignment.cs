//
//   SpecificRoleAssignment.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

namespace Christofel.BaseLib.Database.Models
{
    public class SpecificRoleAssignment
    {
        [Key] public int SpecificRoleAssignmentId { get; set; }

        [MaxLength(32)] public string Name { get; set; } = null!;

        public int AssignmentId { get; set; }

        public RoleAssignment Assignment { get; set; } = null!;
    }
}