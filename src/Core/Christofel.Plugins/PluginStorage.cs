using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Christofel.Plugins.Data;

namespace Christofel.Plugins
{
    /// <summary>
    /// Stores attached and detached plugins thread-safely
    /// </summary>
    public class PluginStorage
    {
        private ImmutableArray<AttachedPlugin> _attachedPlugins;
        private ImmutableArray<DetachedPlugin> _detachedPlugins;

        private object _pluginsLock = new object();

        public PluginStorage()
        {
            _attachedPlugins = ImmutableArray.Create<AttachedPlugin>();
            _detachedPlugins = ImmutableArray.Create<DetachedPlugin>();
        }

        public IReadOnlyCollection<AttachedPlugin> AttachedPlugins => _attachedPlugins;

        public IReadOnlyCollection<DetachedPlugin> DetachedPlugins => _detachedPlugins;

        /// <summary>
        /// Whether plugin with given name is attached
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsAttached(string name) => _attachedPlugins.Any(x => x.Name == name);

        /// <summary>
        /// Returns attached plugin or throws an exception if it is not found
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public AttachedPlugin GetAttachedPlugin(string name) => _attachedPlugins.First(x => x.Name == name);

        /// <summary>
        /// Adds attached plugin
        /// </summary>
        /// <param name="plugin"></param>
        public void AddAttachedPlugin(AttachedPlugin plugin)
        {
            lock (_pluginsLock)
            {
                _attachedPlugins = _attachedPlugins.Add(plugin);
            }
        }

        /// <summary>
        /// Puts AttachedPlugin to detached plugins and removes it from attached plugins
        /// </summary>
        /// <remarks>
        /// Looks up Detached plugin in plugin.DetachedPlugin property, it has to be set
        /// </remarks>
        /// <param name="plugin"></param>
        /// <exception cref="InvalidOperationException"></exception>
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
        /// Removes detached plugin after it's not needed anymore
        /// </summary>
        /// <param name="plugin"></param>
        public void RemoveDetachedPlugin(DetachedPlugin plugin)
        {
            lock (_pluginsLock)
            {
                _detachedPlugins = _detachedPlugins.Remove(plugin);
            }
        }
    }
}