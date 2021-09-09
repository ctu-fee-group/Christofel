//
//   PluginAssemblyService.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Christofel.Plugins.Assemblies;
using Christofel.Plugins.Data;
using Microsoft.Extensions.Logging;

namespace Christofel.Plugins.Services
{
    public class PluginAssemblyService
    {
        private readonly ILogger _logger;

        public PluginAssemblyService(ILogger<PluginAssemblyService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        ///     Load plugin Assembly to memory
        /// </summary>
        /// <param name="path"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public ContextedAssembly AttachAssembly(string path)
        {
            _logger.LogDebug(@"Loading assembly");
            ContextedAssembly info = AssemblyLoader.Load(path);
            _logger.LogDebug(@"Assembly loaded");

            return info;
        }

        /// <summary>
        ///     Creates raw plugin instance
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public IPlugin CreateRawPlugin(ContextedAssembly info)
        {
            _logger.LogDebug(@"Finding IModule");
            Type pluginType = info.Assembly.ExportedTypes.First(x => x.IsAssignableTo(typeof(IPlugin)));
            _logger.LogDebug(@"Found IModule");

            var rawPlugin = (IPlugin?) Activator.CreateInstance(pluginType);
            if (rawPlugin == null)
            {
                _logger.LogDebug(@"Plugin could not be initialized");
                throw new InvalidOperationException("Could not initialize the plugin");
            }

            return rawPlugin;
        }

        public void UnloadPlugin(AttachedPlugin plugin, DetachedPlugin detached)
        {
            WeakReference reference = plugin.Detach();
            detached.AssemblyContextReference = reference;
        }
    }
}