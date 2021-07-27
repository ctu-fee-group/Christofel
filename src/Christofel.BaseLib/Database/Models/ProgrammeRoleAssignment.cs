using System.ComponentModel.DataAnnotations.Schema;

namespace Christofel.BaseLib.Database.Models
{
    public class ProgrammeRoleAssignment
    {
        public string Programme { get; set; } = null!;

        public int AssignmentId { get; set; }
        
        [ForeignKey("AssignmentId")]
        public RoleAssignment Assignment { get; set; } = null!;    }
}