//
//   PluginAutoloader.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Plugins;
using Christofel.Plugins.Runtime;
using Christofel.Plugins.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Application.Plugins
{
    /// <summary>
    /// Stateful service for managing plugins states.
    /// </summary>
    /// <remarks>
    /// Loads plugins on <see cref="IStartable.StartAsync"/> based on the options.
    /// Refreshes plugins on <see cref="IRefreshable.RefreshAsync"/>.
    /// Unloads all plugins on <see cref="IStoppable.StopAsync"/>.
    /// </remarks>
    public class PluginAutoloader : IStartable, IRefreshable, IStoppable
    {
        private readonly ILogger<PluginAutoloader> _logger;
        private readonly PluginAutoloaderOptions _options;
        private readonly PluginService _plugins;
        private readonly PluginStorage _pluginStorage;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginAutoloader"/> class.
        /// </summary>
        /// <param name="options">The options for the autoloading.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="plugins">The service of the plugins.</param>
        /// <param name="pluginStorage">The storage for the plugins.</param>
        public PluginAutoloader
        (
            IOptions<PluginAutoloaderOptions> options,
            ILogger<PluginAutoloader> logger,
            PluginService plugins,
            PluginStorage pluginStorage
        )
        {
            _pluginStorage = pluginStorage;
            _plugins = plugins;
            _options = options.Value;
            _logger = logger;
        }

        /// <inheritdoc />
        public Task RefreshAsync(CancellationToken token = default)
        {
            return Task.WhenAll
            (
                _pluginStorage.AttachedPlugins.Select(x => x.Plugin).OfType<IRuntimePlugin>()
                    .Select(x => x.RefreshAsync(token))
            );
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken token = default)
        {
            if (_options.AutoLoad == null)
            {
                return;
            }

            foreach (string module in _options.AutoLoad)
            {
                try
                {
                    await _plugins.AttachAsync(module, token);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $@"Could not attach autoload module {module}");
                }
            }
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken token = default) => _plugins.DetachAllAsync(token);
    }
}