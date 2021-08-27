using System.ComponentModel.DataAnnotations;

namespace Christofel.Api.Ctu.Database
{
    /// <summary>
    /// Role to be assigned to user
    /// </summary>
    public class AssignRole
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        public long AssignRoleId { get; set; }
        
        /// <summary>
        /// Id of the user to assign the role to
        /// </summary>
        public ulong UserDiscordId { get; set; }
        
        /// <summary>
        /// Id of the guild the user is located in
        /// </summary>
        public ulong GuildDiscordId { get; set; }

        /// <summary>
        /// Id of the role to be added/removed
        /// </summary>
        public ulong RoleId { get; set; }
        
        /// <summary>
        /// Add if true, Remove if false
        /// </summary>
        public bool Add { get; set; }
    }
}