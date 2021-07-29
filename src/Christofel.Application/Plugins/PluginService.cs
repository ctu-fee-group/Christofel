using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Christofel.Application.Assemblies;
using Christofel.Application.Extensions;
using Christofel.BaseLib;
using Christofel.BaseLib.Plugins;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Application.Plugins
{
    public class PluginService : IDisposable, IStoppable, IRefreshable
    {
        private object _runLock = new object();
        
        public static string ModuleNameRegex => "^[a-zA-Z0-9\\.]+$";

        private IDisposable _onOptionsChange;

        private List<AttachedPlugin> _plugins;
        private PluginServiceOptions _options;

        private ILogger<PluginService> _logger;

        public PluginService(IOptionsMonitor<PluginServiceOptions> options, ILogger<PluginService> logger)
        {
            _plugins = new List<AttachedPlugin>();
            _logger = logger;

            _options = options.CurrentValue;
            _onOptionsChange = options.OnChange(o => _options = o);
        }

        public IEnumerable<IHasPluginInfo> AttachedPlugins => _plugins.AsReadOnly();

        public bool IsAttached(string name)
        {
            return _plugins.Any(x => x.Name == name);
        }

        public bool Exists(string name)
        {
            return File.Exists(GetModulePath(name));
        }

        public async Task DetachAllAsync()
        {
            _logger.LogInformation("Detaching all plugins");
            foreach (AttachedPlugin plugin in _plugins)
            {
                await DetachAsync(plugin);
            }
        }

        public Task<IHasPluginInfo> AttachAsync(IChristofelState state, string name)
        {
            if (!Regex.IsMatch(name, ModuleNameRegex))
            {
                throw new InvalidOperationException("Name cannot be accepted.");
            }

            if (IsAttached(name))
            {
                throw new InvalidOperationException("This plugin is already attached");
            }

            if (!Exists(name))
            {
                throw new FileNotFoundException("Could not find module");
            }

            return InternalAttachAsync(state, name);
        }

        public async Task<IHasPluginInfo> DetachAsync(string name)
        {
            AttachedPlugin plugin = _plugins.First(x => x.Name == name);
            await DetachAsync(plugin);
            return new DetachedPlugin(plugin);
        }

        public Task<IHasPluginInfo> ReattachAsync(IChristofelState state, string name)
        {
            return ReattachAsync(state, _plugins.First(x => x.Name == name));
        }

        private Task<IHasPluginInfo> InternalAttachAsync(IChristofelState state, string name)
        {
            using (_logger.BeginScope(@$"Attaching {name} plugin"))
            {
                _logger.LogInformation($@"Attaching {name} plugin");

                string path = GetModulePath(name);
                _logger.LogDebug($@"Loading assembly");
                ContextedAssembly info = AssemblyLoader.Load(path);
                _logger.LogDebug($@"Assembly loaded");
                
                _logger.LogDebug($@"Finding IModule");
                Type pluginType = info.Assembly.GetTypeImplementing<IPlugin>();
                _logger.LogDebug($@"Found IModule");
                IPlugin? rawPlugin = (IPlugin?)Activator.CreateInstance(pluginType);

                if (rawPlugin == null)
                {
                    _logger.LogDebug($@"Plugin could not be initialized");
                    throw new InvalidOperationException("Could not initialize the plugin");
                }

                if (rawPlugin.Name != name)
                {
                    throw new InvalidOperationException("Plugin names do not match");
                }

                return InitializePluginAsync(rawPlugin, info, state);
            }
        }

        private async Task<IHasPluginInfo> InitializePluginAsync(IPlugin rawPlugin, ContextedAssembly info, IChristofelState state)
        {
            _logger.LogDebug($@"Initialization started");
            await rawPlugin.InitAsync(state);
            await rawPlugin.RunAsync();

            AttachedPlugin plugin = new AttachedPlugin(rawPlugin, info);
            lock (_runLock)
            {
                _plugins.Add(plugin);
            }

            _logger.LogInformation($@"Plugin {plugin} was attached successfully");

            return plugin;
        }
        
        private async Task DetachAsync(AttachedPlugin plugin)
        {
            _logger.LogInformation($@"Detaching module {plugin}");
            await plugin.Plugin.StopAsync();
            await plugin.Plugin.DestroyAsync();
            
            plugin.PluginAssembly.Detach();
            lock (_runLock)
            {
                _plugins.Remove(plugin);
            }
        }

        private async Task<IHasPluginInfo> ReattachAsync(IChristofelState state, AttachedPlugin plugin)
        {
            _logger.LogInformation($@"Reattaching plugin {plugin}");
            await DetachAsync(plugin);
            return await AttachAsync(state, plugin.Name);
        }

        public void Dispose()
        {
            _onOptionsChange.Dispose();
        }

        private string GetModulePath(string name)
        {
            return Path.Join(Path.GetFullPath(_options.Folder), name + ".dll");
        }

        public Task StopAsync()
        {
            return DetachAllAsync();
        }

        public Task RefreshAsync()
        {
            return Task.WhenAll(
                _plugins.Select(x => x.Plugin.RefreshAsync())
                );
        }
    }
}