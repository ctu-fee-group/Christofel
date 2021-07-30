using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
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
        private List<DetachedPlugin> _detachedPlugins;
        private PluginServiceOptions _options;

        private ILogger<PluginService> _logger;

        public PluginService(IOptionsMonitor<PluginServiceOptions> options, ILogger<PluginService> logger)
        {
            _plugins = new List<AttachedPlugin>();
            _detachedPlugins = new List<DetachedPlugin>();
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

        public Task DetachAllAsync(CancellationToken token = new CancellationToken())
        {
            token.ThrowIfCancellationRequested();
            _logger.LogInformation("Detaching all plugins");
            return Task.WhenAll(_plugins.Select(x => DetachAsync(x, token)));
        }

        public Task<IHasPluginInfo> AttachAsync(IChristofelState state, string name, CancellationToken token = new CancellationToken())
        {
            token.ThrowIfCancellationRequested();
            
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

            return InternalAttachAsync(state, name, token);
        }

        public Task<IHasPluginInfo> DetachAsync(string name, CancellationToken token = new CancellationToken())
        {
            token.ThrowIfCancellationRequested();
            AttachedPlugin plugin = _plugins.First(x => x.Name == name);
            return DetachAsync(plugin, token);
        }

        public Task<IHasPluginInfo> ReattachAsync(IChristofelState state, string name, CancellationToken token = new CancellationToken())
        {
            token.ThrowIfCancellationRequested();
            return ReattachAsync(state, _plugins.First(x => x.Name == name), token);
        }

        private Task<IHasPluginInfo> InternalAttachAsync(IChristofelState state, string name, CancellationToken token = new CancellationToken())
        {
            token.ThrowIfCancellationRequested();
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
                if (token.IsCancellationRequested)
                {
                    info.Detach();
                    token.ThrowIfCancellationRequested();
                }
                
                IPlugin? rawPlugin = (IPlugin?)Activator.CreateInstance(pluginType);

                if (rawPlugin == null)
                {
                    _logger.LogDebug($@"Plugin could not be initialized");
                    throw new InvalidOperationException("Could not initialize the plugin");
                }

                if (rawPlugin.Name != name)
                {
                    info.Detach();
                    throw new InvalidOperationException("Plugin names do not match");
                }

                return InitializePluginAsync(rawPlugin, info, state, token);
            }
        }

        private async Task<IHasPluginInfo> InitializePluginAsync(IPlugin rawPlugin, ContextedAssembly info, IChristofelState state, CancellationToken token = new CancellationToken())
        {
            _logger.LogDebug($@"Initialization started");
            await rawPlugin.InitAsync(state, token);
            if (token.IsCancellationRequested)
            {
                info.Detach();
                token.ThrowIfCancellationRequested();
            }
            await rawPlugin.RunAsync(token);

            AttachedPlugin plugin = new AttachedPlugin(rawPlugin, info);
            lock (_runLock)
            {
                _plugins.Add(plugin);
            }

            _logger.LogInformation($@"Plugin {plugin} was attached successfully");

            return plugin;
        }

        public void CheckDetached()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            bool remove = false;
            foreach (DetachedPlugin plugin in _detachedPlugins)
            {
                if (plugin.AssemblyContextReference?.IsAlive ?? false)
                {
                    _logger.LogError($@"Plugin {plugin} was not unloaded correctly yet");
                }
                else
                {
                    remove = true;
                    _logger.LogInformation($@"{plugin} was finally unloaded");
                }
            }

            if (remove)
            {
                lock (_runLock)
                {
                    _detachedPlugins.RemoveAll(x => (!x.AssemblyContextReference?.IsAlive) ?? true);
                }
            }
        }
        
        private async Task<IHasPluginInfo> DetachAsync(AttachedPlugin plugin, CancellationToken token = new CancellationToken())
        {
            DetachedPlugin detached = new DetachedPlugin(plugin);
            _logger.LogInformation($@"Detaching module {plugin}");
            await plugin.Plugin.StopAsync(token);
            await plugin.Plugin.DestroyAsync(token);

            WeakReference reference = plugin.Detach();
            detached.AssemblyContextReference = reference;
            
            lock (_runLock)
            {
                _detachedPlugins.Add(detached);
                _plugins.Remove(plugin);
            }
            
            if (reference.IsAlive)
            {
                _logger.LogWarning("The assembly was not detached successfully");
            }

            return detached;
        }

        private async Task<IHasPluginInfo> ReattachAsync(IChristofelState state, AttachedPlugin plugin, CancellationToken token = new CancellationToken())
        {
            _logger.LogInformation($@"Reattaching plugin {plugin}");
            await DetachAsync(plugin, token);
            token.ThrowIfCancellationRequested();
            return await AttachAsync(state, plugin.Name, token);
        }

        public void Dispose()
        {
            _onOptionsChange.Dispose();
        }

        private string GetModulePath(string name)
        {
            return Path.Join(Path.GetFullPath(_options.Folder), name, name + ".dll");
        }

        public Task StopAsync(CancellationToken token = new CancellationToken())
        {
            return DetachAllAsync(token);
        }

        public Task RefreshAsync(CancellationToken token = new CancellationToken())
        {
            return Task.WhenAll(
                _plugins.Select(x => x.Plugin.RefreshAsync(token))
                );
        }
    }
}