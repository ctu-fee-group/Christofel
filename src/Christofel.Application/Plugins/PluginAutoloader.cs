using System;
using System.Threading.Tasks;
using Christofel.BaseLib;
using Christofel.BaseLib.Plugins;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Application.Plugins
{
    public class PluginAutoloader : IStartable
    {
        private readonly PluginServiceOptions _options;
        private readonly PluginService _plugins;
        private readonly IChristofelState _state;
        private readonly ILogger<PluginAutoloader> _logger;
        
        public PluginAutoloader(IOptions<PluginServiceOptions> options,
            ILogger<PluginAutoloader> logger,
            PluginService plugins,
            IChristofelState state)
        {
            _state = state;
            _plugins = plugins;
            _options = options.Value;
            _logger = logger;
        }
        
        public async Task StartAsync()
        {
            foreach (string module in _options.AutoLoad)
            {
                try
                {
                    await _plugins.AttachAsync(_state, module);
                }
                catch (Exception e)
                {
                    _logger.LogError(1, $@"Could not attach autoload module {module}", e);
                }
            }
        }
    }
}