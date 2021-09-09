using System;
using System.Collections.Generic;

#nullable disable

namespace Christofel.DatabaseMigrator.Model
{
    public partial class User
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string DiscordId { get; set; }
        public string DiscordUser { get; set; }
        public string CtuUsername { get; set; }
        public bool Duplicity { get; set; }
        public bool DuplicityApproved { get; set; }
        public long? Karma { get; set; }
        public bool Authorized { get; set; }
        public string AuthCode { get; set; }
    }
}
