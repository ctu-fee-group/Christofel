using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Plugins.Assemblies;
using Christofel.Plugins.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Plugins.Services
{
    /// <summary>
    /// Service used for attaching and detaching plugins
    /// </summary>
    public class PluginService : IDisposable
    {
        public static string ModuleNameRegex => "^[a-zA-Z0-9\\.]+$";

        private readonly IDisposable _onOptionsChange;

        private PluginServiceOptions _options;

        private readonly ILogger<PluginService> _logger;
        private readonly PluginStorage _storage;
        private readonly PluginLifetimeService _lifetimeService;
        private readonly PluginAssemblyService _assemblyService;

        public PluginService(
            IOptionsMonitor<PluginServiceOptions> options,
            PluginStorage storage,
            ILogger<PluginService> logger,
            PluginAssemblyService assemblyService,
            PluginLifetimeService lifetimeService)
        {
            _assemblyService = assemblyService;
            _storage = storage;
            _logger = logger;
            _lifetimeService = lifetimeService;

            _options = options.CurrentValue;
            _onOptionsChange = options.OnChange(o => _options = o);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns>If plugin with given name is attached</returns>
        public bool IsAttached(string name)
        {
            return _storage.IsAttached(name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns>If plugin file exists</returns>
        public bool Exists(string name)
        {
            return File.Exists(GetModulePath(name));
        }

        /// <summary>
        /// Returns names of plugins that can be attached (excluding these that are attached)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAttachablePluginNames()
        {
            string pluginsPath = Path.GetFullPath(_options.Folder);
            return Directory.GetDirectories(pluginsPath)
                .Select(Path.GetFileName)
                .Where(x => x is not null)
                .Cast<string>()
                .Except(_storage.AttachedPlugins.Select(x => x.Name));
        }

        /// <summary>
        /// Detached all attached plugins
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task DetachAllAsync(CancellationToken token = new CancellationToken())
        {
            token.ThrowIfCancellationRequested();
            _logger.LogInformation("Detaching all plugins");
            return Task.WhenAll(_storage.AttachedPlugins
                .ToList()
                .Select(x => InternalDetachAsync(x, token)));
        }

        /// <summary>
        /// Attaches plugin given by name
        /// </summary>
        /// <param name="state">State to initialize the plugin with</param>
        /// <param name="name">Name of the plugin to search for</param>
        /// <param name="token">Cancel token</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public Task<IHasPluginInfo> AttachAsync(string name, CancellationToken token = new CancellationToken())
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

            return InternalAttachAsync(name, token);
        }

        /// <summary>
        /// Tries to detach plugin given by name
        /// </summary>
        /// <param name="name">Name of the plugin</param>
        /// <param name="token">Cancel token</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Task<IHasPluginInfo> DetachAsync(string name, CancellationToken token = new CancellationToken())
        {
            if (!IsAttached(name))
            {
                throw new InvalidOperationException("This plugin is not attached");
            }

            token.ThrowIfCancellationRequested();
            AttachedPlugin plugin = _storage.GetAttachedPlugin(name);
            return InternalDetachAsync(plugin, token);
        }

        /// <summary>
        /// Tries to detach and then attach a plugin given by name
        /// </summary>
        /// <remarks>
        /// If the plugin fails to detach in time, InvalidOperationException is thrown
        /// </remarks>
        /// <param name="state">State to initialize new plugin with</param>
        /// <param name="name"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Task<IHasPluginInfo> ReattachAsync(string name,
            CancellationToken token = new CancellationToken())
        {
            if (!IsAttached(name))
            {
                throw new InvalidOperationException("This plugin is not attached");
            }

            token.ThrowIfCancellationRequested();
            return InternalReattachAsync(_storage.GetAttachedPlugin(name), token);
        }

        private async Task<IHasPluginInfo> InternalAttachAsync(string name,
            CancellationToken token = new CancellationToken())
        {
            token.ThrowIfCancellationRequested();
            using (_logger.BeginScope(@$"Attaching {name} plugin"))
            {
                _logger.LogInformation($@"Attaching {name} plugin");

                ContextedAssembly assembly = _assemblyService.AttachAssembly(GetModulePath(name));
                IPlugin rawPlugin = _assemblyService.CreateRawPlugin(assembly);

                if (rawPlugin.Name != name)
                {
                    assembly.Detach();
                    throw new InvalidOperationException("Plugin names do not match");
                }

                if (token.IsCancellationRequested)
                {
                    assembly.Detach();
                    token.ThrowIfCancellationRequested();
                }

                var attached = new AttachedPlugin(rawPlugin, assembly);
                _logger.LogDebug($@"Initialization started");
                var initialized = await _lifetimeService.InitializeAsync(attached, token);

                if (initialized)
                {
                    _storage.AddAttachedPlugin(attached);
                }
                
                _logger.LogInformation($@"Plugin {attached} was initialized successfully");

                return attached;
            }
        }

        /// <summary>
        /// Checks memory used by detached plugins, reports using logging only
        /// </summary>
        public void CheckDetached()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            foreach (DetachedPlugin plugin in _storage.DetachedPlugins.ToList())
            {
                if (plugin.AssemblyContextReference?.IsAlive ?? false)
                {
                    _logger.LogError($@"Plugin {plugin} was not unloaded correctly yet");
                }
                else
                {
                    _storage.RemoveDetachedPlugin(plugin);
                    _logger.LogInformation($@"{plugin} was finally unloaded");
                }
            }
        }

        private async Task<IHasPluginInfo> InternalDetachAsync(AttachedPlugin plugin,
            CancellationToken token = new CancellationToken())
        {
            DetachedPlugin detached = new DetachedPlugin(plugin);

            if (plugin.DetachedPlugin != null)
            {
                return plugin.DetachedPlugin;
            }

            plugin.DetachedPlugin = detached;
            _storage.DetachAttachedPlugin(plugin);

            await _lifetimeService.DestroyAsync(plugin, token);
            
            _assemblyService.UnloadPlugin(plugin, detached);

            return detached;
        }

        private async Task<IHasPluginInfo> InternalReattachAsync(AttachedPlugin plugin,
            CancellationToken token = new CancellationToken())
        {
            _logger.LogInformation($@"Reattaching plugin {plugin}");
            DetachedPlugin detached = (DetachedPlugin)await InternalDetachAsync(plugin, token);
            token.ThrowIfCancellationRequested();

            if (!detached.Destroyed)
            {
                _logger.LogError("Cannot finish reattach as the plugin did not stop in time");
                throw new InvalidOperationException("Could not detach the plugin in time, aborting reattach");
            }

            return await AttachAsync(detached.Name, token);
        }

        public void Dispose()
        {
            _onOptionsChange.Dispose();
        }

        private string GetModulePath(string name)
        {
            return Path.Join(Path.GetFullPath(_options.Folder), name, name + ".dll");
        }
    }
}