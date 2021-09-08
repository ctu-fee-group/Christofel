using Christofel.BaseLib.Lifetime;

namespace Christofel.BaseLib.Plugins
{
    /// <summary>
    /// Should be used for lifetime of the whole application.
    /// </summary>
    public interface IApplicationLifetime : ILifetime<IChristofelState> {}
    
    /// <summary>
    /// Should be used for lifetime of the current plugin.
    /// </summary>
    /// <remarks>
    /// Current plugin means the one of which the current instance of service etc. is part of
    /// </remarks>
    public interface ICurrentPluginLifetime : ILifetime<IPlugin> {}
}