//
//   PluginLifetimeService.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Plugins.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.Plugins.Services
{
    /// <summary>
    /// Lifetime service that will distribute initialization and destroyal of plugins to matching LifetimeServices.
    /// </summary>
    public class PluginLifetimeService : IPluginLifetimeService
    {
        private readonly IServiceProvider _services;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginLifetimeService"/> class.
        /// </summary>
        /// <param name="services">The provider of the services.</param>
        public PluginLifetimeService(IServiceProvider services)
        {
            _services = services;
        }

        /// <inheritdoc />
        public bool ShouldHandle(IPlugin plugin) => true;

        /// <inheritdoc />
        public Task<bool> InitializeAsync
            (AttachedPlugin plugin, CancellationToken token = default) => GetLifetimeService
            (plugin.Plugin).InitializeAsync(plugin, token);

        /// <inheritdoc />
        public Task<bool> DestroyAsync
            (AttachedPlugin plugin, CancellationToken token = default) => GetLifetimeService(plugin.Plugin).DestroyAsync
            (plugin, token);

        /// <summary>
        /// Gets matching lifetime service for the specified plugin.
        /// </summary>
        /// <param name="plugin">The plugin to get lifetime service for.</param>
        /// <returns>The lifetime service matching this plugin.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no matching service was found.</exception>
        private IPluginLifetimeService GetLifetimeService(IPlugin plugin)
        {
            return _services.GetServices<IPluginLifetimeService>().First(x => x.ShouldHandle(plugin));
        }
    }
}