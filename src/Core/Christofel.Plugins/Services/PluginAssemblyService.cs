//
//   PluginAssemblyService.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Runtime.Loader;
using Christofel.Plugins.Assemblies;
using Christofel.Plugins.Data;
using Microsoft.Extensions.Logging;

namespace Christofel.Plugins.Services
{
    /// <summary>
    /// Service for loading and unloading plugins into the memory using <see cref="AssemblyLoadContext"/>.
    /// </summary>
    public class PluginAssemblyService
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginAssemblyService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public PluginAssemblyService(ILogger<PluginAssemblyService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Load plugin Assembly into memory using <see cref="AssemblyLoadContext"/>.
        /// </summary>
        /// <param name="path">The path to the plugin assembly.</param>
        /// <returns>The assembly with the context if was loaded into.</returns>
        public ContextedAssembly AttachAssembly(string path)
        {
            _logger.LogDebug(@"Loading assembly");
            ContextedAssembly info = AssemblyLoader.Load(path);
            _logger.LogDebug(@"Assembly loaded");

            return info;
        }

        /// <summary>
        /// Creates raw plugin instance from assembly information.
        /// </summary>
        /// <param name="info">Information about the assembly.</param>
        /// <returns>The plugin that was found inside of the assembly.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the plugin class could not be found.</exception>
        public IPlugin CreateRawPlugin(ContextedAssembly info)
        {
            _logger.LogDebug(@"Finding IModule");
            Type pluginType = info.Assembly.ExportedTypes.First(x => x.IsAssignableTo(typeof(IPlugin)));
            _logger.LogDebug(@"Found IModule");

            var rawPlugin = (IPlugin?)Activator.CreateInstance(pluginType);
            if (rawPlugin == null)
            {
                _logger.LogDebug(@"Plugin could not be initialized");
                throw new InvalidOperationException("Could not initialize the plugin");
            }

            return rawPlugin;
        }

        /// <summary>
        /// Unloads plugin from memory and saves reference to the <see cref="AssemblyLoadContext"/> to the specified detached plugin.
        /// </summary>
        /// <param name="plugin">The plugin that should be detached from the memory.</param>
        /// <param name="detached">The representation of the plugin where ALC will be saved into.</param>
        public void UnloadPlugin(AttachedPlugin plugin, DetachedPlugin detached)
        {
            WeakReference reference = plugin.Detach();
            detached.AssemblyContextReference = reference;
        }
    }
}