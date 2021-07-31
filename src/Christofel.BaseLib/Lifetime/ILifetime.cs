using System;
using System.Threading;
using Christofel.BaseLib.Plugins;

namespace Christofel.BaseLib.Lifetime
{
    public interface ILifetime
    {
        public LifetimeState State { get; }
        
        public CancellationToken Errored { get; }
        public CancellationToken Started { get; }
        public CancellationToken Stopped { get; }
        public CancellationToken Stopping { get; }
        
        public void RequestStop();
    }

    public interface ILifetime<T> : ILifetime { }
    
    public interface IApplicationLifetime : ILifetime<IChristofelState> {}
    public interface ICurrentPluginLifetime : ILifetime<IPlugin> {}
}