using System;
using System.Collections.Generic;
using System.Linq;

namespace Christofel.Application.Plugins
{
    public class PluginStorage
    {
        private List<AttachedPlugin> _attachedPlugins;
        private List<DetachedPlugin> _detachedPlugins;

        private object _pluginsLock = new object();
        
        public PluginStorage()
        {
            _attachedPlugins = new List<AttachedPlugin>();
            _detachedPlugins = new List<DetachedPlugin>();
        }

        public IReadOnlyCollection<AttachedPlugin> AttachedPlugins => _attachedPlugins.AsReadOnly();
        public IReadOnlyCollection<DetachedPlugin> DetachedPlugins => _detachedPlugins.AsReadOnly();

        public bool IsAttached(string name)
        {
            lock (_pluginsLock)
            {
                return _attachedPlugins.Any(x => x.Name == name);
            }
        }

        public AttachedPlugin GetAttachedPlugin(string name)
        {
            lock (_pluginsLock)
            {
                return _attachedPlugins.First(x => x.Name == name);
            }
        }

        public void AddAttachedPlugin(AttachedPlugin plugin)
        {
            lock (_pluginsLock)
            {
                _attachedPlugins.Add(plugin);
            }
        }

        public void DetachAttachedPlugin(AttachedPlugin plugin)
        {
            if (plugin.DetachedPlugin == null)
            {
                throw new InvalidOperationException("Plugin is not detached");
            }
            
            lock (_pluginsLock)
            {
                if (_attachedPlugins.Contains(plugin))
                {
                    _attachedPlugins.Remove(plugin);
                }

                if (!_detachedPlugins.Contains(plugin.DetachedPlugin))
                {
                    _detachedPlugins.Add(plugin.DetachedPlugin);
                }
            }
        }

        public void RemoveDetachedPlugin(DetachedPlugin plugin)
        {
            lock (_pluginsLock)
            {
                _detachedPlugins.Remove(plugin);
            }
        }
        
    }
}