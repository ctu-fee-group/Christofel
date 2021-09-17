//
//   UsermapRoleAssignment.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Christofel.Common.Database.Models
{
    /// <summary>
    /// Database table used for assignments using Usermap roles.
    /// </summary>
    [Table("UsermapRoleAssignment", Schema = ChristofelBaseContext.SchemaName)]
    public class UsermapRoleAssignment
    {
        /// <summary>
        /// Gets or sets primary key of <see cref="UsermapRoleAssignment"/>.
        /// </summary>
        [Key]
        public int UsermapRoleAssignmentId { get; set; }

        /// <summary>
        /// Gets or sets the name of the role or regex matching roles.
        /// </summary>
        [MaxLength(512)]
        public string UsermapRole { get; set; } = null!;

        /// <summary>
        /// Gets or sets whether the match should be done using regex or equality comparison.
        /// </summary>
        /// <remarks>
        /// If true, match by regex.
        /// If false, match the whole string.
        /// </remarks>
        public bool RegexMatch { get; set; }

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