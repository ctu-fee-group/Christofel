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
    /// Represents an attached plugin with all information about it.
    /// </summary>
    public class AttachedPlugin : IHasPluginInfo
    {
        private IPlugin? _plugin;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttachedPlugin"/> class.
        /// </summary>
        /// <param name="plugin">Attached plugin instance.</param>
        /// <param name="assembly">Assembly the plugin is inside of.</param>
        public AttachedPlugin(IPlugin plugin, ContextedAssembly assembly)
        {
            _plugin = plugin;
            PluginAssembly = assembly;
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Gets attached Plugin state.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws when the plugin was already detached using <see cref="Detach"/>.</exception>
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
        /// Gets instance of the detached plugin, if this plugin was already detached.
        /// </summary>
        /// <remarks>
        /// After detaching the plugin, information cannot be obtained by using <see cref="Plugin"/>,
        /// but only by using this property.
        /// </remarks>
        public DetachedPlugin? DetachedPlugin { get; set; }

        /// <summary>
        /// Unique id of the plugin to check against DetachedPlugins.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Assembly where the plugin is loaded.
        /// </summary>
        public ContextedAssembly PluginAssembly { get; }

        /// <inheritdoc />
        public string Name => Plugin.Name;

        /// <inheritdoc />
        public string Description => Plugin.Description;

        /// <inheritdoc />
        public string Version => Plugin.Version;

        /// <inheritdoc />
        public override string ToString() => $@"{Name} ({Version})";

        /// <summary>
        /// Detaches the ContextedAssembly.
        /// Removes references to the plugin,
        /// it should be destroyed (or at least notified about stopping).
        /// </summary>
        /// <returns>Weak reference to AssemblyLoadContext so it can be checked whether the AssemblyLoadContext was destroyed.</returns>
        public WeakReference Detach()
        {
            _plugin = null;
            return PluginAssembly.Detach();
        }
    }
}