namespace Christofel.Plugins
{
    /// <summary>
    /// Options for loading plugins
    /// </summary>
    public class PluginServiceOptions
    {
        /// <summary>
        /// Folder where to look for plugins
        /// </summary>
        public string Folder { get; set; } = null!;
    }
}