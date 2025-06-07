//
//   IStoppable.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

namespace Christofel.Plugins.Runtime
{
    /// <summary>
    /// Represents an entity that supports stopping on the application or plugin stop.
    /// </summary>
    public interface IStoppable
    {
        /// <summary>
        /// Stops the operation of the entity.
        /// </summary>
        /// <param name="token">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task StopAsync(CancellationToken token = default);
    }
}