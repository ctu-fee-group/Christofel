using System;
using System.ComponentModel.DataAnnotations;
using Christofel.BaseLib.Database.Models.Abstractions;
using Christofel.BaseLib.User;

namespace Christofel.BaseLib.Database.Models
{
    public class DbUser : ITimestampsEntity, ILinkUser
    {
        [Key]
        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        public DateTime? AuthenticatedAt { get; set; }

        public ulong DiscordId { get; set; }

        public string CtuUsername { get; set; } = null!;
        
        public bool Duplicity { get; set; }
        
        public bool DuplicityApproved { get; set; }
    }
}