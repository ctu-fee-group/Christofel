namespace Christofel.BaseLib.Plugins
{
    public class PluginContext : IPluginContext
    {
        public IEveryResponder? PluginResponder { get; set; }
    }
}