//
//   IRuntimePlugin.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Christofel.Plugins.Lifetime;

namespace Christofel.Plugins
{
#pragma warning disable SA1402 // FileMayOnlyContainASingleType
    /// <summary>
    /// Runtime plugin with a <see cref="ILifetime"/> for managing the state of the plugin.
    /// </summary>
    public interface IRuntimePlugin : IPlugin
    {
        /// <summary>
        /// Gets lifetime of the plugin allowing to stop the plugin
        /// and check its state.
        /// </summary>
        public ILifetime Lifetime { get; }

        /// <summary>
        /// Starts the operation of the plugin.
        /// </summary>
        /// <remarks>
        /// If the running operation blocks, another thread should be created.
        /// </remarks>
        /// <param name="token">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task RunAsync(CancellationToken token = default);

        /// <summary>
        /// Refreshes the plugin configuration.
        /// </summary>
        /// <param name="token">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task RefreshAsync(CancellationToken token = default);
    }

    /// <summary>
    /// Runtime plugin receiving a state of the application along with a context of the plugin.
    /// </summary>
    /// <typeparam name="TState">The state of the application that is given to the plugin.</typeparam>
    /// <typeparam name="TContext">The context of the plugin that will be given to the application.</typeparam>
    public interface IRuntimePlugin<in TState, TContext> : IRuntimePlugin
    {
        /// <summary>
        /// Gets context of this plugin.
        /// </summary>
        public TContext Context { get; }

        /// <summary>
        /// Initializes the services of this plugin.
        /// </summary>
        /// <param name="state">The state of application.</param>
        /// <param name="token">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task InitAsync(TState state, CancellationToken token = default);
    }
}