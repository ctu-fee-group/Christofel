using System;
using System.Collections.Generic;

#nullable disable

namespace Christofel.DatabaseMigrator.Model
{
    public partial class Command
    {
        public Command()
        {
            CommandPermissions = new HashSet<CommandPermission>();
        }

        public long Id { get; set; }
        public byte[] CreatedAt { get; set; }
        public byte[] UpdatedAt { get; set; }
        public byte[] DeletedAt { get; set; }
        public string Name { get; set; }
        public byte[] Public { get; set; }
        public byte[] Autoremove { get; set; }
        public string Description { get; set; }

        public virtual ICollection<CommandPermission> CommandPermissions { get; set; }
    }
}
