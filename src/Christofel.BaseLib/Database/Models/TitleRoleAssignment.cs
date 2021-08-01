using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Christofel.BaseLib.Database.Models
{
    /// <summary>
    /// Database table for assignment based on titles in their name
    /// </summary>
    public class TitleRoleAssignment
    {
        [Key]
        public int TitleRoleAssignmentId { get; set; }
        
        [MaxLength(32)]
        public string Title { get; set; } = null!;
        
        public bool Post { get; set; }
        
        public bool Pre { get; set; }
        
        public uint Priority { get; set; }

        [ForeignKey("Assignment")]
        public int AssignmentId { get; set; }
        
        [InverseProperty("TitleRoleAssignments")]
        public RoleAssignment Assignment { get; set; } = null!;
    }
}