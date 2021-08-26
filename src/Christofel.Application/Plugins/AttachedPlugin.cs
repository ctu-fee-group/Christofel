using System;
using Christofel.Application.Assemblies;
using Christofel.BaseLib.Plugins;

namespace Christofel.Application.Plugins
{
    /// <summary>
    /// Represents an attached plugin
    /// holding its state
    /// </summary>
    public class AttachedPlugin : IHasPluginInfo
    {
        private IPlugin? _plugin;
        private IPluginContext? _pluginContext;
        
        public AttachedPlugin(IPlugin plugin, IPluginContext context, ContextedAssembly assembly)
        {
            _plugin = plugin;
            _pluginContext = context;
            PluginAssembly = assembly;
            Id = Guid.NewGuid();
        }

        public IPluginContext Context
        {
            get
            {
                if (_pluginContext is null)
                {
                    throw new InvalidOperationException("Plugin was already detached");
                }

                return _pluginContext;
            }
        }

        /// <summary>
        /// Attached Plugin state
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
        /// If the plugin was already Detached, this symbols it
        /// </summary>
        /// <remarks>
        /// This is here because we need to check if the plugin
        /// was already detached in Lifetime callbacks sometimes
        /// </remarks>
        public DetachedPlugin? DetachedPlugin { get; set; }
        
        /// <summary>
        /// Unique id of the plugin to check against DetachedPlugins
        /// </summary>
        public Guid Id { get; }
        
        /// <summary>
        /// Assembly where the plugin is loaded
        /// </summary>
        public ContextedAssembly PluginAssembly { get; }

        public string Name => Plugin.Name;
        public string Description => Plugin.Description;
        public string Version => Plugin.Version;

        public override string ToString()
        {
            return $@"{Name} ({Version})";
        }

        /// <summary>
        /// Detaches the ContextedAssembly.
        /// Removes references to the plugin,
        /// it should be destroyed (or at least notified about stopping)
        /// </summary>
        /// <returns></returns>
        public WeakReference Detach()
        {
            _plugin = null;
            _pluginContext = null;
            return PluginAssembly.Detach();
        }
    }
}