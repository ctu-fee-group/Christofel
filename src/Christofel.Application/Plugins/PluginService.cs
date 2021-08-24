using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Application.Assemblies;
using Christofel.BaseLib;
using Christofel.BaseLib.Plugins;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Application.Plugins
{
    /// <summary>
    /// Service used for attaching and detaching plugins
    /// </summary>
    public class PluginService : IDisposable, IStoppable, IRefreshable
    {
        public static string ModuleNameRegex => "^[a-zA-Z0-9\\.]+$";

        private readonly IDisposable _onOptionsChange;

        private PluginServiceOptions _options;

        private readonly ILogger<PluginService> _logger;
        private readonly PluginStorage _storage;
        private readonly PluginLifetimeService _lifetimeService;

        public PluginService(
            IOptionsMonitor<PluginServiceOptions> options,
            PluginLifetimeService lifetimeService,
            PluginStorage storage,
            ILogger<PluginService> logger)
        {
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
        public Task<IHasPluginInfo> ReattachAsync(IChristofelState state, string name, CancellationToken token = new CancellationToken())
        {
            if (!IsAttached(name))
            {
                throw new InvalidOperationException("This plugin is not attached");
            }

            token.ThrowIfCancellationRequested();
            return InternalReattachAsync(state, _storage.GetAttachedPlugin(name), token);
        }

        private async Task<IHasPluginInfo> InternalAttachAsync(IChristofelState state, string name, CancellationToken token = new CancellationToken())
        {
            token.ThrowIfCancellationRequested();
            using (_logger.BeginScope(@$"Attaching {name} plugin"))
            {
                _logger.LogInformation($@"Attaching {name} plugin");

                ContextedAssembly assembly = await _lifetimeService.AttachAssemblyAsync(GetModulePath(name));
                IPlugin rawPlugin = await _lifetimeService.CreateRawPluginAsync(assembly);

                if (rawPlugin.Name != name)
                {
                    assembly.Detach();
                    rawPlugin.Lifetime.RequestStop();
                    throw new InvalidOperationException("Plugin names do not match");
                }

                return await _lifetimeService.InitializePluginAsync(rawPlugin, assembly, state, token);
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
        
        private async Task<IHasPluginInfo> InternalDetachAsync(AttachedPlugin plugin, CancellationToken token = new CancellationToken())
        {
            DetachedPlugin detached = new DetachedPlugin(plugin);
            
            if (plugin.DetachedPlugin != null)
            {
                return plugin.DetachedPlugin;
            }

            plugin.DetachedPlugin = detached;
            _storage.DetachAttachedPlugin(plugin);

            await _lifetimeService.DetachPluginAsync(plugin, detached);

            return detached;
        }
        
        private async Task<IHasPluginInfo> InternalReattachAsync(IChristofelState state, AttachedPlugin plugin, CancellationToken token = new CancellationToken())
        {
            _logger.LogInformation($@"Reattaching plugin {plugin}");
            DetachedPlugin detached = (DetachedPlugin)await InternalDetachAsync(plugin, token);
            token.ThrowIfCancellationRequested();

            if (!detached.Destroyed)
            {
                _logger.LogError("Cannot finish reattach as the plugin did not stop in time");
                throw new InvalidOperationException("Could not detach the plugin in time, aborting reattach");
            }
            
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

        /// <summary>
        /// Detaches all plugins
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken token = new CancellationToken())
        {
            return DetachAllAsync(token);
        }

        /// <summary>
        /// Refreshes all plugins
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task RefreshAsync(CancellationToken token = new CancellationToken())
        {
            return Task.WhenAll(
                _storage.AttachedPlugins.Select(x => x.Plugin.RefreshAsync(token)));
        }
    }
}