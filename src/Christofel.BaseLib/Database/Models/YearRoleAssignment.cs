using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Christofel.BaseLib.Database.Models
{
    public class YearRoleAssignment
    {
        [Key]
        public int YearRoleAssignmentId { get; set; }
        
        public int Year { get; set; }
        
        [ForeignKey("Assignment")]
        public int AssignmentId { get; set; }
        
        [InverseProperty("YearRoleAssignments")]
        public RoleAssignment Assignment { get; set; } = null!;
    }
}