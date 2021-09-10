//
//   IPluginLifetimeService.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Christofel.Plugins.Data;

namespace Christofel.Plugins.Services
{
    /// <summary>
    /// Service supporting initialization and destruction of some plugins.
    /// </summary>
    public interface IPluginLifetimeService
    {
        /// <summary>
        /// Tells if the specified plugin can be handled by this service.
        /// </summary>
        /// <param name="plugin">The plugin in question.</param>
        /// <returns>Whether the plugin should be handled by this service.</returns>
        public bool ShouldHandle(IPlugin plugin);

        /// <summary>
        /// Initializes the given plugin after it was attached.
        /// </summary>
        /// <param name="plugin">The plugin to be initialized.</param>
        /// <param name="token">The cancellation token for this operation.</param>
        /// <returns>Whether the initialization was successful.</returns>
        public Task<bool> InitializeAsync(AttachedPlugin plugin, CancellationToken token = default);

        /// <summary>
        /// Destroys the given plugin before detach.
        /// </summary>
        /// <param name="plugin">The plugin to be destroyed.</param>
        /// <param name="token">The cancellation token for this operation.</param>
        /// <returns>Whether the plugin was correctly destroyed.</returns>
        public Task<bool> DestroyAsync(AttachedPlugin plugin, CancellationToken token = default);
    }
}