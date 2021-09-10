//
//   PluginService.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
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
    /// Service used for attaching, detaching plugins along with distributing handling of their lifetime to PluginLifetimeService.
    /// </summary>
    public class PluginService : IDisposable
    {
        private readonly PluginAssemblyService _assemblyService;
        private readonly PluginLifetimeService _lifetimeService;

        private readonly ILogger<PluginService> _logger;

        private readonly IDisposable _onOptionsChange;
        private readonly PluginStorage _storage;

        private PluginServiceOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginService"/> class.
        /// </summary>
        /// <param name="options">The options for attaching plugins.</param>
        /// <param name="storage">The storage for the plugins.</param>
        /// <param name="logger">The logger used for logging error states.</param>
        /// <param name="assemblyService">The service for loading and unloading plugin assemblies from memory.</param>
        /// <param name="lifetimeService">The service for handling initialization and destroyal of plugins.</param>
        public PluginService
        (
            IOptionsMonitor<PluginServiceOptions> options,
            PluginStorage storage,
            ILogger<PluginService> logger,
            PluginAssemblyService assemblyService,
            PluginLifetimeService lifetimeService
        )
        {
            _assemblyService = assemblyService;
            _storage = storage;
            _logger = logger;
            _lifetimeService = lifetimeService;

            _options = options.CurrentValue;
            _onOptionsChange = options.OnChange(o => _options = o);
        }

        /// <summary>
        /// Regex that all plugin names must match.
        /// </summary>
        public static string ModuleNameRegex => "^[a-zA-Z0-9\\.]+$";

        /// <inheritdoc />
        public void Dispose()
        {
            _onOptionsChange.Dispose();
        }

        /// <summary>
        /// Gets whether plugin with given name is attached.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <returns>If plugin with given name is attached.</returns>
        public bool IsAttached(string name) => _storage.IsAttached(name);

        /// <summary>
        /// Gets whether plugin assembly with given name exists.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <returns>If plugin assembly with given name exists.</returns>
        public bool Exists(string name) => File.Exists(GetModulePath(name));

        /// <summary>
        /// Gets names of plugins that can be attached (excluding these that are attached).
        /// </summary>
        /// <returns>Names of plugins that can be attached.</returns>
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
        /// Detached all attached plugins at once.
        /// </summary>
        /// <param name="token">The cancellation token for the operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task DetachAllAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            _logger.LogInformation("Detaching all plugins");
            return Task.WhenAll
            (
                _storage.AttachedPlugins
                    .ToList()
                    .Select(x => InternalDetachAsync(x, token))
            );
        }

        /// <summary>
        /// Attaches plugin given by name.
        /// </summary>
        /// <remarks>
        /// Loads plugin into memory and initializes it using <see cref="PluginLifetimeService"/>.
        /// </remarks>
        /// <param name="name">The name of the plugin to search for.</param>
        /// <param name="token">The cancellation token for this operation.</param>
        /// <returns>Complete information about the attached plugin.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the name cannot be accepted or if the plugin was already attached.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the plugin does not exist.</exception>
        public Task<IHasPluginInfo> AttachAsync(string name, CancellationToken token = default)
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
        /// Detaches plugin given by name.
        /// </summary>
        /// <remarks>
        /// Destroys the plugin using <see cref="PluginLifetimeService"/> and unloads it from the memory.
        /// </remarks>
        /// <param name="name">The name of the plugin.</param>
        /// <param name="token">The cancellation token for this operation.</param>
        /// <returns>Complete information about the detached plugin.</returns>
        /// <exception cref="InvalidOperationException">Thrown if plugin with the given name is not attached.</exception>
        public Task<IHasPluginInfo> DetachAsync(string name, CancellationToken token = default)
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
        /// Reattaches the given plugin by detaching it and then attaching it again.
        /// </summary>
        /// <remarks>
        /// If the plugin fails to detach in time, it will be in an undefined state.
        /// </remarks>
        /// <param name="name">The name of the plugin to reattach.</param>
        /// <param name="token">The cancellation token for this operation.</param>
        /// <returns>Complete information about the attached plugin.</returns>
        /// <exception cref="InvalidOperationException">Thrown if plugin with the given name is not attached.</exception>
        public Task<IHasPluginInfo> ReattachAsync
        (
            string name,
            CancellationToken token = default
        )
        {
            if (!IsAttached(name))
            {
                throw new InvalidOperationException("This plugin is not attached");
            }

            token.ThrowIfCancellationRequested();
            return InternalReattachAsync(_storage.GetAttachedPlugin(name), token);
        }

        /// <summary>
        /// Checks whether detached plugins were removed from the memory.
        /// </summary>
        /// <remarks>
        /// Forces garbage collection, iterates over detached plugins and checks whether reference
        /// to <see cref="AssemblyLoadContext"/> was lost already. Logs the results into <see cref="ILogger"/>.
        /// </remarks>
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

        private async Task<IHasPluginInfo> InternalAttachAsync
        (
            string name,
            CancellationToken token = default
        )
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
                _logger.LogDebug(@"Initialization started");
                var initialized = false;

                try
                {
                    initialized = await _lifetimeService.InitializeAsync(attached, token);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Could not attach {attached}");
                }

                if (initialized)
                {
                    _storage.AddAttachedPlugin(attached);
                    _logger.LogInformation($@"Plugin {attached} was initialized successfully");
                }
                else
                {
                    _assemblyService.UnloadPlugin(attached, new DetachedPlugin(attached));
                }

                return attached;
            }
        }

        private async Task<IHasPluginInfo> InternalDetachAsync
        (
            AttachedPlugin plugin,
            CancellationToken token = default
        )
        {
            DetachedPlugin detached = new DetachedPlugin(plugin);

            if (plugin.DetachedPlugin != null)
            {
                return plugin.DetachedPlugin;
            }

            plugin.DetachedPlugin = detached;
            _storage.DetachAttachedPlugin(plugin);

            detached.DestroyedInTime = await _lifetimeService.DestroyAsync(plugin, token);

            _assemblyService.UnloadPlugin(plugin, detached);

            return detached;
        }

        private async Task<IHasPluginInfo> InternalReattachAsync
        (
            AttachedPlugin plugin,
            CancellationToken token = default
        )
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

        private string GetModulePath(string name) => Path.Join(Path.GetFullPath(_options.Folder), name, name + ".dll");
    }
}