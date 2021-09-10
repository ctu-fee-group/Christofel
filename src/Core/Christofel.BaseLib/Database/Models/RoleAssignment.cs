//
//   RoleAssignment.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Christofel.BaseLib.Database.Models.Enums;
using Remora.Discord.Core;

namespace Christofel.BaseLib.Database.Models
{
    public class RoleAssignment
    {
        [Key] public int RoleAssignmentId { get; set; }

        /// <summary>
        /// Discord role id
        /// </summary>
        public Snowflake RoleId { get; set; }

        /// <summary>
        /// Type of the role for special behaviors
        /// </summary>
        public RoleType RoleType { get; set; }

        public virtual ICollection<SpecificRoleAssignment>? SpecificRoleAssignments { get; set; }
        public virtual ICollection<YearRoleAssignment>? YearRoleAssignments { get; set; }
        public virtual ICollection<ProgrammeRoleAssignment>? ProgrammeRoleAssignments { get; set; }
        public virtual ICollection<TitleRoleAssignment>? TitleRoleAssignments { get; set; }
        public virtual ICollection<UsermapRoleAssignment>? UsermapRoleAssignments { get; set; }
    }
}