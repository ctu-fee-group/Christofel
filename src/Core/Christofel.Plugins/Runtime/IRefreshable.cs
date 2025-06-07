//
//  IRefreshable.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

namespace Christofel.Plugins.Runtime
{
    /// <summary>
    /// Represents an entity that supports refreshing on the application or plugin refresh.
    /// </summary>
    public interface IRefreshable
    {
        /// <summary>
        /// Refreshes data of the entity.
        /// </summary>
        /// <remarks>
        /// Used for refreshing data from the database or configuration such as permissions.
        /// </remarks>
        /// <param name="token">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task RefreshAsync(CancellationToken token = default);
    }
}