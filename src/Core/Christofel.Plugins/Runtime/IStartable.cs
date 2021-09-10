//
//   IStartable.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

namespace Christofel.Plugins.Runtime
{
    /// <summary>
    /// Represents an entity that supports starting on the application or plugin start.
    /// </summary>
    public interface IStartable
    {
        /// <summary>
        /// Starts the operation of the entity.
        /// </summary>
        /// <remarks>
        /// Should start the operation, if the operation
        /// will be blocking, it should start in a separate thread.
        /// </remarks>
        /// <param name="token">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task StartAsync(CancellationToken token = default);
    }
}