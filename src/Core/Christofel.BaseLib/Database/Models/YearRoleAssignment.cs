using System.ComponentModel.DataAnnotations;

namespace Christofel.BaseLib.Database.Models
{
    /// <summary>
    /// Database table for assignments using year the student started studying
    /// </summary>
    public class YearRoleAssignment
    {
        [Key]
        public int YearRoleAssignmentId { get; set; }
        
        public int Year { get; set; }
        
        public int AssignmentId { get; set; }
        
        public RoleAssignment Assignment { get; set; } = null!;
    }
}