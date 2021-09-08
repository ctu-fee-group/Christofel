using System;
using System.Collections.Generic;

#nullable disable

namespace Christofel.DatabaseMigrator.Model
{
    public partial class User
    {
        public long Id { get; set; }
        public byte[] CreatedAt { get; set; }
        public byte[] UpdatedAt { get; set; }
        public string DiscordId { get; set; }
        public string DiscordUser { get; set; }
        public string CtuUsername { get; set; }
        public byte[] Duplicity { get; set; }
        public byte[] DuplicityApproved { get; set; }
        public long? Karma { get; set; }
        public byte[] Authorized { get; set; }
        public string AuthCode { get; set; }
    }
}
