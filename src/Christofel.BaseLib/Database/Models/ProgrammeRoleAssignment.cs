using System.ComponentModel.DataAnnotations;

namespace Christofel.BaseLib.Database.Models
{
    /// <summary>
    /// Database table for assignment of roles based on programme the student is studying.
    /// </summary>
    public class ProgrammeRoleAssignment
    {
        [Key]
        public int ProgrammeRoleAssignmentId { get; set; }
        
        [MaxLength(256)]
        public string Programme { get; set; } = null!;

        public int AssignmentId { get; set; }
        
        public RoleAssignment Assignment { get; set; } = null!;
    }
}