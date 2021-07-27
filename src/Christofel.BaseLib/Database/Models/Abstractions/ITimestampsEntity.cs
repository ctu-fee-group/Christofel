using System;

namespace Christofel.BaseLib.Database.Models.Abstractions
{
    public interface ITimestampsEntity
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}