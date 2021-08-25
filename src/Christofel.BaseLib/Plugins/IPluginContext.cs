namespace Christofel.BaseLib.Plugins
{
    /// <summary>
    /// Context of attached plugin
    /// </summary>
    public interface IPluginContext
    {
        /// <summary>
        /// Responder that will be called for every event if not null
        /// </summary>
        public IEveryResponder? PluginResponder { get; }
    }
}