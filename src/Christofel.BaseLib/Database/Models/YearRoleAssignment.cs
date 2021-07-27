using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Christofel.BaseLib.Database.Models
{
    public class YearRoleAssignment
    {
        [Key]
        public int YearRoleAssignmentId { get; set; }
        
        public int Year { get; set; }
        
        public int AssignmentId { get; set; }
        
        [ForeignKey("AssignmentId")]
        public RoleAssignment Assignment { get; set; } = null!;
    }
}