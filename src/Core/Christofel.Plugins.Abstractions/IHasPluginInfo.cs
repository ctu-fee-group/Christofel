namespace Christofel.Plugins
{
    /// <summary>
    /// Abstraction of plugin info to be able to use info about plugin even after plugin was destroyed
    /// </summary>
    public interface IHasPluginInfo
    {
        public string Name { get; }
        public string Description { get; }
        public string Version { get; }
    }
}