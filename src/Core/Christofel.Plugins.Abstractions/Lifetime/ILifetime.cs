using System.Threading;

namespace Christofel.Plugins.Lifetime
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
        /// In case of an error this state is set
        /// </summary>
        /// <remarks>
        /// This may be set in any stage of the lifetime and indicates
        /// critical failure where the plugin will not be able to recover
        /// itself from it.
        /// </remarks>
        public bool IsErrored { get; }
        
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
}