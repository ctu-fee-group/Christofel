//
//   ProgrammeRoleAssignment.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Christofel.Common.Database.Models
{
    /// <summary>
    /// Database table for assignment of roles based on programme the student is studying.
    /// </summary>
    [Table("ProgrammeRoleAssignment", Schema = ChristofelBaseContext.SchemaName)]
    public class ProgrammeRoleAssignment
    {
        /// <summary>
        /// Gets or sets primary key of the <see cref="ProgrammeRoleAssignment"/>.
        /// </summary>
        [Key]
        public int ProgrammeRoleAssignmentId { get; set; }

        /// <summary>
        /// Gets or sets the name of the programme.
        /// </summary>
        [MaxLength(256)]
        public string Programme { get; set; } = null!;

        /// <summary>
        /// Gets or sets id of the assignment.
        /// </summary>
        public int? AssignmentId { get; set; }

        /// <summary>
        /// Gets or sets the assignment.
        /// </summary>
        public RoleAssignment? Assignment { get; set; } = null!;
    }
}