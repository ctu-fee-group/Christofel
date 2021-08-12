using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Christofel.BaseLib.Database.Models.Enums;

namespace Christofel.BaseLib.Database.Models
{
    public class RoleAssignment
    {
        [Key]
        public int RoleAssignmentId { get; set; }

        /// <summary>
        /// Discord role id
        /// </summary>
        public ulong RoleId { get; set; }

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