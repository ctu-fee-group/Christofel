using System.ComponentModel.DataAnnotations;

namespace Christofel.BaseLib.Database.Models
{
    /// <summary>
    /// Database table that holds
    /// permission assignments to discord targets.
    /// </summary>
    /// <remarks>
    /// Wildcards may be used
    ///
    /// <example>
    /// For example,
    ///
    /// Suppose we have permissions <c>management.messages.slowmode</c>, <c>management.users.create</c>
    /// Permission <c>management.*</c> will grant both of these permissions at once.
    ///
    /// </example>
    /// </remarks>
    public class PermissionAssignment
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        public int PermissionAssignmentId { get; set; }

        /// <summary>
        /// Name of the permission to be granted (wildcards may be used)
        /// </summary>
        [MaxLength(512)]
        public string PermissionName { get; set; } = null!;

        /// <summary>
        /// Target to assign this permission to
        /// </summary>
        public DiscordTarget Target { get; set; } = null!;
    }
}