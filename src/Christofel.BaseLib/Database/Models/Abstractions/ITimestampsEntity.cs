using System;

namespace Christofel.BaseLib.Database.Models.Abstractions
{
    /// <summary>
    /// Entity having CreatedAt and UpdatedAt that will be automatically updated by the Context/db
    /// </summary>
    public interface ITimestampsEntity
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}