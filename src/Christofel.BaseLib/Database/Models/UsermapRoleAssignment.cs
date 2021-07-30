using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Christofel.BaseLib.Database.Models
{
    public class UsermapRoleAssignment
    {
        [Key]
        public int UsermapRoleAssignmentId { get; set; }
        
        [MaxLength(512)]
        public string UsermapRole { get; set; } = null!;
        
        public bool RegexMatch { get; set; }
        
        [ForeignKey("Assignment")]
        public int AssignmentId { get; set; }
        
        [InverseProperty("UsermapRoleAssignments")]
        public RoleAssignment Assignment { get; set; } = null!;
    }
}