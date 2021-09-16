//
//   TitleRoleAssignment.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Christofel.BaseLib.Database.Models
{
    /// <summary>
    /// Database table used for assignment based on titles in their name.
    /// </summary>
    [Table("TitleRoleAssignment", Schema = ChristofelBaseContext.SchemaName)]
    public class TitleRoleAssignment
    {
        /// <summary>
        /// Gets or sets primary key of <see cref="TitleRoleAssignment"/>.
        /// </summary>
        [Key]
        public int TitleRoleAssignmentId { get; set; }

        /// <summary>
        /// Gets or sets matching title of the assignment.
        /// </summary>
        [MaxLength(32)]
        public string Title { get; set; } = null!;

        /// <summary>
        /// Gets or sets whether the title should be after the name.
        /// </summary>
        public bool Post { get; set; }

        /// <summary>
        /// Gets or sets whether the title should be in front of the name.
        /// </summary>
        public bool Pre { get; set; }

        /// <summary>
        /// Gets or sets the priority of current title.
        /// </summary>
        /// <remarks>
        /// If user has multiple matching titles, the one with the most priority will be used.
        /// If there are multiple roles with the same priority, all of them will be assigned.
        /// </remarks>
        public uint Priority { get; set; }

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