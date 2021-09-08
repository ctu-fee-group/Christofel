using System;
using System.Collections.Generic;

#nullable disable

namespace Christofel.DatabaseMigrator.Model
{
    public partial class MessageChannel
    {
        public long Id { get; set; }
        public byte[] CreatedAt { get; set; }
        public byte[] UpdatedAt { get; set; }
        public byte[] DeletedAt { get; set; }
        public string MessageId { get; set; }
        public string EmojiId { get; set; }
        public string ChannelId { get; set; }
    }
}
