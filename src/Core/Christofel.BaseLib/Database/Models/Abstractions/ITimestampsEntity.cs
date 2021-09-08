using System;

namespace Christofel.BaseLib.Database.Models.Abstractions
{
    /// <summary>
    /// Entity having CreatedAt and UpdatedAt that will be automatically updated by the Context/db
    /// </summary>
    public interface ITimestampsEntity
    {
        /// <summary>
        /// Date and time of creation of the entity
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Date and time of update of the entity, set if the entity was updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}