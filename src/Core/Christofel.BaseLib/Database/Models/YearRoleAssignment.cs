//
//   YearRoleAssignment.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

namespace Christofel.BaseLib.Database.Models
{
    /// <summary>
    /// Database table used for assignments using year the student started studying.
    /// </summary>
    public class YearRoleAssignment
    {
        /// <summary>
        /// Gets or sets primary key of <see cref="YearRoleAssignment"/>.
        /// </summary>
        [Key]
        public int YearRoleAssignmentId { get; set; }

        /// <summary>
        /// Gets or sets the matching year.
        /// </summary>
        public int Year { get; set; }

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