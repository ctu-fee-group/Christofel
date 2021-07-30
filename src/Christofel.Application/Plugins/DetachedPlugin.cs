using System;
using Christofel.BaseLib.Plugins;

namespace Christofel.Application.Plugins
{
    public class DetachedPlugin : IHasPluginInfo
    {
        public DetachedPlugin(IHasPluginInfo plugin)
        {
            Name = plugin.Name;
            Description = plugin.Description;
            Version = plugin.Version;
        }
        
        public WeakReference? AssemblyContextReference { get; internal set; }

        public string Name { get; }
        public string Description { get; }
        public string Version { get; }
        
        public override string ToString()
        {
            return $@"{Name} ({Version})";
        }
    }
}