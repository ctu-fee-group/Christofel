using System.ComponentModel.DataAnnotations;

namespace Christofel.BaseLib.Database.Models
{
    public class SpecificRoleAssignment
    {
        [Key]
        public int SpecificRoleAssignmentId { get; set; }
        
        [MaxLength(32)]
        public string Name { get; set; } = null!;

        public int AssignmentId { get; set; }
        
        public RoleAssignment Assignment { get; set; } = null!;
    }
}