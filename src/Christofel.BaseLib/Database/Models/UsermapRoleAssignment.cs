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
        
        public bool Contain { get; set; }

        public int AssignmentId { get; set; }
        
        [ForeignKey("AssignmentId")]
        public RoleAssignment Assignment { get; set; } = null!;    }
}