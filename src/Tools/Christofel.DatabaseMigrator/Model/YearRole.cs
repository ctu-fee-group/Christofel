#nullable disable

namespace Christofel.DatabaseMigrator.Model
{
    public partial class YearRole
    {
        public long Id { get; set; }
        public byte[] CreatedAt { get; set; }
        public byte[] UpdatedAt { get; set; }
        public byte[] DeletedAt { get; set; }
        public long? Year { get; set; }
        public string RoleId { get; set; }
    }
}
