//
//   PluginStorage.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Christofel.Plugins.Data;

namespace Christofel.Plugins
{
    /// <summary>
    /// Stores attached and detached plugins thread-safely.
    /// </summary>
    public class PluginStorage
    {
        private readonly object _pluginsLock = new object();
        private ImmutableArray<AttachedPlugin> _attachedPlugins;
        private ImmutableArray<DetachedPlugin> _detachedPlugins;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginStorage"/> class.
        /// </summary>
        public PluginStorage()
        {
            _attachedPlugins = ImmutableArray.Create<AttachedPlugin>();
            _detachedPlugins = ImmutableArray.Create<DetachedPlugin>();
        }

        /// <summary>
        /// Gets plugins that are currently attached.
        /// </summary>
        public IReadOnlyCollection<AttachedPlugin> AttachedPlugins => _attachedPlugins;

        /// <summary>
        /// Gets plugins that were recently detached, but were not unloaded from the memory yet.
        /// </summary>
        public IReadOnlyCollection<DetachedPlugin> DetachedPlugins => _detachedPlugins;

        /// <summary>
        /// Gets whether plugin with given name is attached.
        /// </summary>
        /// <param name="name">The name of the plugin to search for.</param>
        /// <returns>Whether the plugin is attached.</returns>
        public bool IsAttached(string name) => _attachedPlugins.Any(x => x.Name == name);

        /// <summary>
        /// Gets attached plugin or throws an exception if it is not found.
        /// </summary>
        /// <param name="name">The name of the plugin to get.</param>
        /// <returns>The plugin that was found.</returns>
        public AttachedPlugin GetAttachedPlugin(string name) => _attachedPlugins.First(x => x.Name == name);

        /// <summary>
        /// Adds attached plugin to the storage.
        /// </summary>
        /// <param name="plugin">The plugin to add.</param>
        public void AddAttachedPlugin(AttachedPlugin plugin)
        {
            lock (_pluginsLock)
            {
                _attachedPlugins = _attachedPlugins.Add(plugin);
            }
        }

        /// <summary>
        /// Puts AttachedPlugin to detached plugins and removes it from attached plugins.
        /// </summary>
        /// <remarks>
        /// Looks up Detached plugin in plugin.DetachedPlugin property, it has to be set.
        /// </remarks>
        /// <param name="plugin">The plugin to detach.</param>
        /// <exception cref="InvalidOperationException">Thrown if the <see cref="AttachedPlugin.DetachedPlugin"/> is null.</exception>
        public void DetachAttachedPlugin(AttachedPlugin plugin)
        {
            if (plugin.DetachedPlugin == null)
            {
                throw new InvalidOperationException("Plugin is not detached");
            }

            lock (_pluginsLock)
            {
                _attachedPlugins = _attachedPlugins.Remove(plugin);

                if (!_detachedPlugins.Contains(plugin.DetachedPlugin))
                {
                    _detachedPlugins = _detachedPlugins.Add(plugin.DetachedPlugin);
                }
            }
        }

        /// <summary>
        /// Removes detached plugin after it's unloaded from the memory completely.
        /// </summary>
        /// <param name="plugin">The plugin that should be removed.</param>
        public void RemoveDetachedPlugin(DetachedPlugin plugin)
        {
            lock (_pluginsLock)
            {
                _detachedPlugins = _detachedPlugins.Remove(plugin);
            }
        }
    }
}