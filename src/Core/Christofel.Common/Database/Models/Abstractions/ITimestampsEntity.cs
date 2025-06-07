//
//   ITimestampsEntity.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Christofel.Common.Database.Models.Abstractions
{
    /// <summary>
    /// Entity having CreatedAt and UpdatedAt that will be automatically updated by the context.
    /// </summary>
    public interface ITimestampsEntity
    {
        /// <summary>
        /// Date and time of creation of the entity.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date and time of update of the entity, set if the entity was updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}