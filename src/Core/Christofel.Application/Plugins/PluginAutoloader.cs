using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib;
using Christofel.BaseLib.Plugins;
using Christofel.Plugins;
using Christofel.Plugins.Runtime;
using Christofel.Plugins.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Application.Plugins
{
    /// <summary>
    /// Auto loads plugins specified in configuration
    /// on startup
    /// </summary>
    public class PluginAutoloader : IStartable, IRefreshable, IStoppable
    {
        private readonly PluginAutoloaderOptions _options;
        private readonly PluginStorage _pluginStorage;
        private readonly PluginService _plugins;
        private readonly ILogger<PluginAutoloader> _logger;

        public PluginAutoloader(IOptions<PluginAutoloaderOptions> options,
            ILogger<PluginAutoloader> logger,
            PluginService plugins,
            PluginStorage pluginStorage)
        {
            _pluginStorage = pluginStorage;
            _plugins = plugins;
            _options = options.Value;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken token = new CancellationToken())
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

        public Task RefreshAsync(CancellationToken token = new CancellationToken())
        {
            return Task.WhenAll(_pluginStorage.AttachedPlugins.Select(x => x.Plugin).OfType<IRuntimePlugin>()
                .Select(x => x.RefreshAsync(token)));
        }

        public Task StopAsync(CancellationToken token = new CancellationToken())
        {
            return _plugins.DetachAllAsync(token);
        }
    }
}