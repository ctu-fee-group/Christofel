using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Christofel.BaseLib.Database.Models.Abstractions;
using Christofel.BaseLib.User;

namespace Christofel.BaseLib.Database.Models
{
    /// <summary>
    /// Database table that holds authenticated users
    /// or users in auth process.
    /// </summary>
    public class DbUser : ITimestampsEntity, ILinkUser
    {
        [Key]
        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        /// <summary>
        /// Last date of authentication
        /// </summary>
        public DateTime? AuthenticatedAt { get; set; }

        /// <summary>
        /// Id of the user on Discord
        /// </summary>
        public ulong DiscordId { get; set; }

        /// <summary>
        /// CTU account username
        /// </summary>
        [MaxLength(256)]
        public string CtuUsername { get; set; } = null!;

        /// <summary>
        /// When this user is a duplicity (DuplicitUser is not null)
        /// then set this to true if this user is allowed to finish the auth process
        /// </summary>
        public bool DuplicityApproved { get; set; }
        
        /// <summary>
        /// Id of the user this is a duplicity with
        /// </summary>
        [ForeignKey("DuplicitUser")]
        public int? DuplicitUserId { get; set; }
        
        public DbUser? DuplicitUser { get; set; }
    }
}