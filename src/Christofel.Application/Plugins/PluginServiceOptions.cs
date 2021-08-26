namespace Christofel.Application.Plugins
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

        /// <summary>
        /// What plugins to autoload when starting
        /// </summary>
        public string[]? AutoLoad { get; set; } = null!;
    }
}