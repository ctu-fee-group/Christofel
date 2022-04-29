#nullable disable

namespace Christofel.DatabaseMigrator.Model
{
    public partial class MessageRole
    {
        public long Id { get; set; }
        public byte[] CreatedAt { get; set; }
        public byte[] UpdatedAt { get; set; }
        public byte[] DeletedAt { get; set; }
        public string MessageId { get; set; }
        public string EmojiId { get; set; }
        public string RoleId { get; set; }

        public virtual Role Role { get; set; }
    }
}
