using System;
using Christofel.Application.Assemblies;
using Christofel.BaseLib.Plugins;

namespace Christofel.Application.Plugins
{
    public class AttachedPlugin : IHasPluginInfo
    {
        private IPlugin? _plugin;
        
        public AttachedPlugin(IPlugin plugin, ContextedAssembly assembly)
        {
            _plugin = plugin;
            PluginAssembly = assembly;
        }

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
        
        public ContextedAssembly PluginAssembly { get; }

        public string Name => Plugin.Name;
        public string Description => Plugin.Description;
        public string Version => Plugin.Version;

        public override string ToString()
        {
            return $@"{Name} ({Version})";
        }

        public WeakReference Detach()
        {
            _plugin = null;
            return PluginAssembly.Detach();
        }
    }
}