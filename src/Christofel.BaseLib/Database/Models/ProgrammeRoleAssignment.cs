using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Christofel.BaseLib.Database.Models
{
    public class ProgrammeRoleAssignment
    {
        [Key]
        public int ProgrammeRoleAssignmentId { get; set; }
        
        public string Programme { get; set; } = null!;

        [ForeignKey("Assignment")]
        public int AssignmentId { get; set; }
        
        [InverseProperty("ProgrammeRoleAssignments")]
        public RoleAssignment Assignment { get; set; } = null!;
    }
}