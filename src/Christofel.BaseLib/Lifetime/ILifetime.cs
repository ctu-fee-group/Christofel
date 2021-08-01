using System;
using System.Threading;
using Christofel.BaseLib.Plugins;

namespace Christofel.BaseLib.Lifetime
{
    /// <summary>
    /// Interface for providing lifetime support
    /// to various services.
    /// </summary>
    public interface ILifetime
    {
        /// <summary>
        /// Current state of the service.
        /// </summary>
        /// <remarks>
        /// Should be always set to the current state.
        /// This field may be used for mechanisms of waiting for specific state
        /// </remarks>
        public LifetimeState State { get; }
        
        /// <summary>
        /// Errored token is canceled in case of an error
        /// </summary>
        public CancellationToken Errored { get; }
        
        /// <summary>
        /// Custom callbacks may be registered.
        /// Started token is canceled after going to Running state.
        /// </summary>
        public CancellationToken Started { get; }
        
        /// <summary>
        /// Custom callbacks may be registered.
        /// Stopped token is canceled after going to Stopped state.
        /// The service will not be destroyed until all callbacks finish. 
        /// </summary>
        public CancellationToken Stopped { get; }
        
        /// <summary>
        /// Custom callbacks may be registered.
        /// Stopping token is canceled before stopping.
        /// The service will not stop until all callbacks finish.
        /// </summary>
        public CancellationToken Stopping { get; }
        
        /// <summary>
        /// Requests a stop that should be obeyed if it's possible.
        /// </summary>
        /// <remarks>
        /// This is just a request for a stop, it should not be blocking for long.
        /// Actual stop will be done in the nearest future.
        /// No time can be guaranteed, but generally it should be ASAP
        /// </remarks>
        public void RequestStop();
    }

    /// <summary>
    /// Specific lifetime type.
    /// May be usable for DependencyInjection.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ILifetime<T> : ILifetime { }
    
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