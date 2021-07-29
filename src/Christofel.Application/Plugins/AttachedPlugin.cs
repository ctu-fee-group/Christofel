using Christofel.Application.Assemblies;
using Christofel.BaseLib.Plugins;

namespace Christofel.Application.Plugins
{
    public class AttachedPlugin : IHasPluginInfo
    {
        public AttachedPlugin(IPlugin plugin, ContextedAssembly assembly)
        {
            Plugin = plugin;
            PluginAssembly = assembly;
        }
        
        public IPlugin Plugin { get; }
        
        public ContextedAssembly PluginAssembly { get; }

        public string Name => Plugin.Name;
        public string Description => Plugin.Description;
        public string Version => Plugin.Version;

        public override string ToString()
        {
            return $@"{Name} ({Version})";
        }
    }
}