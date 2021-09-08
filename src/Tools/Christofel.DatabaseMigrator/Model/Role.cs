using System;
using System.Collections.Generic;

#nullable disable

namespace Christofel.DatabaseMigrator.Model
{
    public partial class Role
    {
        public Role()
        {
            CommandPermissions = new HashSet<CommandPermission>();
            MessageRoles = new HashSet<MessageRole>();
        }

        public long Id { get; set; }
        public byte[] CreatedAt { get; set; }
        public byte[] UpdatedAt { get; set; }
        public byte[] DeletedAt { get; set; }
        public string DiscordId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<CommandPermission> CommandPermissions { get; set; }
        public virtual ICollection<MessageRole> MessageRoles { get; set; }
    }
}
