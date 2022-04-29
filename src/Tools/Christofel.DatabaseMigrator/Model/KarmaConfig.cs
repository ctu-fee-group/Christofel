using System.Collections.Generic;

#nullable disable

namespace Christofel.DatabaseMigrator.Model
{
    public partial class KarmaConfig
    {
        public KarmaConfig()
        {
            MessageKarmaReports = new HashSet<MessageKarmaReport>();
        }

        public long Id { get; set; }
        public byte[] CreatedAt { get; set; }
        public byte[] UpdatedAt { get; set; }
        public byte[] DeletedAt { get; set; }
        public string EmojiId { get; set; }
        public long? Effect { get; set; }
        public long? Trigger { get; set; }

        public virtual ICollection<MessageKarmaReport> MessageKarmaReports { get; set; }
    }
}
