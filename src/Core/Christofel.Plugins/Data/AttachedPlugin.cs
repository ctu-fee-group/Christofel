//
//   AttachedPlugin.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Christofel.Plugins.Assemblies;

namespace Christofel.Plugins.Data
{
    /// <summary>
    ///     Represents an attached plugin
    ///     holding its state
    /// </summary>
    public class AttachedPlugin : IHasPluginInfo
    {
        private IPlugin? _plugin;

        public AttachedPlugin(IPlugin plugin, ContextedAssembly assembly)
        {
            _plugin = plugin;
            PluginAssembly = assembly;
            Id = Guid.NewGuid();
        }

        /// <summary>
        ///     Attached Plugin state
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public IPlugin Plugin
        {
            get
            {
                if (_plugin == null)
                {
                    throw new InvalidOperationException("Plugin was already detached");
                }

                return _plugin;
            }
        }

        /// <summary>
        ///     If the plugin was already Detached, this symbols it
        /// </summary>
        /// <remarks>
        ///     This is here because we need to check if the plugin
        ///     was already detached in Lifetime callbacks sometimes
        /// </remarks>
        public DetachedPlugin? DetachedPlugin { get; set; }

        /// <summary>
        ///     Unique id of the plugin to check against DetachedPlugins
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        ///     Assembly where the plugin is loaded
        /// </summary>
        public ContextedAssembly PluginAssembly { get; }

        public string Name => Plugin.Name;
        public string Description => Plugin.Description;
        public string Version => Plugin.Version;

        public override string ToString() => $@"{Name} ({Version})";

        /// <summary>
        ///     Detaches the ContextedAssembly.
        ///     Removes references to the plugin,
        ///     it should be destroyed (or at least notified about stopping)
        /// </summary>
        /// <returns></returns>
        public WeakReference Detach()
        {
            _plugin = null;
            return PluginAssembly.Detach();
        }
    }
}