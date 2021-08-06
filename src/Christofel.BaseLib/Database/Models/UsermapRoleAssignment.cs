using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Christofel.BaseLib.Database.Models
{
    /// <summary>
    /// Database table for assginments using Usermap roles
    /// </summary>
    public class UsermapRoleAssignment
    {
        [Key]
        public int UsermapRoleAssignmentId { get; set; }
        
        [MaxLength(512)]
        public string UsermapRole { get; set; } = null!;
        
        /// <summary>
        /// If true, match by regex.
        /// If false, match the whole string.
        /// </summary>
        public bool RegexMatch { get; set; }
        
        public int AssignmentId { get; set; }
        
        public RoleAssignment Assignment { get; set; } = null!;
    }
}