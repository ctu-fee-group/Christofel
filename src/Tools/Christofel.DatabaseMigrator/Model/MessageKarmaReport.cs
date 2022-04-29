#nullable disable

namespace Christofel.DatabaseMigrator.Model
{
    public partial class MessageKarmaReport
    {
        public long Id { get; set; }
        public byte[] CreatedAt { get; set; }
        public byte[] UpdatedAt { get; set; }
        public byte[] DeletedAt { get; set; }
        public string MessageId { get; set; }
        public long? KarmaConfigId { get; set; }

        public virtual KarmaConfig KarmaConfig { get; set; }
    }
}
