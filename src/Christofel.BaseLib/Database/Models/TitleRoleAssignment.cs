using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Christofel.BaseLib.Database.Models
{
    public class TitleRoleAssignment
    {
        [Key]
        public int TitleRoleAssignmentId { get; set; }
        
        [MaxLength(32)]
        public string Title { get; set; } = null!;
        
        public bool Post { get; set; }
        
        public bool Pre { get; set; }

        public int AssignmentId { get; set; }
        
        [ForeignKey("AssignmentId")]
        public RoleAssignment Assignment { get; set; } = null!;    }
}