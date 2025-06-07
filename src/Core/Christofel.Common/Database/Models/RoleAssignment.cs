//
//   RoleAssignment.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Christofel.Common.Database.Models.Enums;
using Remora.Rest.Core;

namespace Christofel.Common.Database.Models
{
    /// <summary>
    /// Database table that holds roles that will be assigned during auth process.
    /// </summary>
    [Table("RoleAssignment", Schema = ChristofelBaseContext.SchemaName)]
    public class RoleAssignment
    {
        /// <summary>
        /// Gets or sets primary key of <see cref="RoleAssignment"/>.
        /// </summary>
        [Key]
        public int RoleAssignmentId { get; set; }

        /// <summary>
        /// Gets or sets Discord id of the role.
        /// </summary>
        public Snowflake RoleId { get; set; }

        /// <summary>
        /// Gets or rests the type of the role for special behaviors.
        /// </summary>
        public RoleType RoleType { get; set; }

        /// <summary>
        /// Gets or sets specific role assignments that reference this assignment.
        /// </summary>
        public virtual ICollection<SpecificRoleAssignment>? SpecificRoleAssignments { get; set; }

        /// <summary>
        /// Gets or sets year role assignments that reference this assignment.
        /// </summary>
        public virtual ICollection<YearRoleAssignment>? YearRoleAssignments { get; set; }

        /// <summary>
        /// Gets or sets programme role assignments that reference this assignment.
        /// </summary>
        public virtual ICollection<ProgrammeRoleAssignment>? ProgrammeRoleAssignments { get; set; }

        /// <summary>
        /// Gets or sets title role assignments that reference this assignment.
        /// </summary>
        public virtual ICollection<TitleRoleAssignment>? TitleRoleAssignments { get; set; }

        /// <summary>
        /// Gets or sets usermap role assignments that reference this assignment.
        /// </summary>
        public virtual ICollection<UsermapRoleAssignment>? UsermapRoleAssignments { get; set; }
    }
}