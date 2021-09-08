using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Plugins.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.Plugins.Services
{
    public class PluginLifetimeService : IPluginLifetimeService
    {
        private readonly IServiceProvider _services;
        
        public PluginLifetimeService(IServiceProvider services)
        {
            _services = services;
        }
        
        public bool ShouldHandle(IPlugin plugin)
        {
            return true;
        }

        public Task<bool> InitializeAsync(AttachedPlugin plugin, CancellationToken token = default)
        {
            return GetLifetimeService(plugin.Plugin).InitializeAsync(plugin, token);
        }

        public Task<bool> DestroyAsync(AttachedPlugin plugin, CancellationToken token = default)
        {
            return GetLifetimeService(plugin.Plugin).DestroyAsync(plugin, token);
        }
        
        private IPluginLifetimeService GetLifetimeService(IPlugin plugin)
        {
            return _services.GetServices<IPluginLifetimeService>().First(x => x.ShouldHandle(plugin));
        }
    }
}