using System.ComponentModel.DataAnnotations;

namespace Christofel.BaseLib.Database.Models
{
    public class PermissionAssignment
    {
        [Key]
        public int PermissionAssignmentId { get; set; }

        public string PermissionName { get; set; } = null!;

        public DiscordTarget Target { get; set; } = null!;
    }
}