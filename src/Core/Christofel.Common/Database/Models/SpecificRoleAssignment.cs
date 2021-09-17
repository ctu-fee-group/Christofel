//
//   SpecificRoleAssignment.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Christofel.Common.Database.Models
{
    /// <summary>
    /// Database table for assignments matched by specific name.
    /// </summary>
    [Table("SpecificRoleAssignment", Schema = ChristofelBaseContext.SchemaName)]
    public class SpecificRoleAssignment
    {
        /// <summary>
        /// Gets or sets primary key of <see cref="SpecificRoleAssignment"/>.
        /// </summary>
        [Key]
        public int SpecificRoleAssignmentId { get; set; }

        /// <summary>
        /// The name of the specific role used for matching.
        /// </summary>
        [MaxLength(32)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Gets or sets referenced assignment id.
        /// </summary>
        public int AssignmentId { get; set; }

        /// <summary>
        /// Gets or sets referenced assignment.
        /// </summary>
        public RoleAssignment Assignment { get; set; } = null!;
    }
}