//
//   HandleReactType.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.ReactHandler.Database.Models
{
    /// <summary>
    /// Type of entity that should be added/removed.
    /// </summary>
    public enum HandleReactType
    {
        /// <summary>
        /// Represents that the <see cref="HandleReact.EntityId"/> is role.
        /// </summary>
        Role,

        /// <summary>
        /// Represents that the <see cref="HandleReact.EntityId"/> is channel.
        /// </summary>
        Channel,
    }
}