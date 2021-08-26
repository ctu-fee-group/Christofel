namespace Christofel.BaseLib.Plugins
{
    public class PluginContext : IPluginContext
    {
        public IAnyResponder? PluginResponder { get; set; }
    }
}