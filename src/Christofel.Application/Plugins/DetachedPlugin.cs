using System;
using Christofel.BaseLib.Plugins;

namespace Christofel.Application.Plugins
{
    public class DetachedPlugin : IHasPluginInfo
    {
        public DetachedPlugin(AttachedPlugin plugin)
        {
            Name = plugin.Name;
            Description = plugin.Description;
            Version = plugin.Version;
            Id = plugin.Id;
        }
        
        public Guid Id { get; }
        
        public WeakReference? AssemblyContextReference { get; internal set; }

        public bool Destroyed => DestroyedLate || DestroyedInTime;
        
        public bool DestroyedLate { get; set; }
        public bool DestroyedInTime { get; set; }
        public string Name { get; }
        public string Description { get; }
        public string Version { get; }
        
        public override string ToString()
        {
            return $@"{Name} ({Version})";
        }
    }
}