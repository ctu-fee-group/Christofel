#nullable disable

namespace Christofel.DatabaseMigrator.Model
{
    public partial class CommandPermission
    {
        public long Id { get; set; }
        public byte[] CreatedAt { get; set; }
        public byte[] UpdatedAt { get; set; }
        public byte[] DeletedAt { get; set; }
        public long? CommandId { get; set; }
        public string RoleId { get; set; }

        public virtual Command Command { get; set; }
        public virtual Role Role { get; set; }
    }
}
