#nullable disable

namespace Christofel.DatabaseMigrator.Model
{
    public partial class ProgrammeRole
    {
        public long Id { get; set; }
        public byte[] CreatedAt { get; set; }
        public byte[] UpdatedAt { get; set; }
        public byte[] DeletedAt { get; set; }
        public string ProgrammeName { get; set; }
        public string RoleId { get; set; }
    }
}
