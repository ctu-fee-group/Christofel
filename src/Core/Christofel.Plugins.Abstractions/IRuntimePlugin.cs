using System.Threading;
using System.Threading.Tasks;
using Christofel.Plugins.Lifetime;

namespace Christofel.Plugins
{
    /// <summary>
    /// Interface for implementing plugin
    ///
    /// Plugins are attached during runtime to allow changing parts of the application in runtime
    ///
    /// Plugin lifetime goes from uninitialized, initialized (InitAsync), running (RunAsync), stopped (StopAsync), destroyed (DestroyAsync).
    /// Separation of Init/Run and Stop/Destroy is not needed, but chosen to better separate what happens where
    /// </summary>
    public interface IRuntimePlugin<in TState, TContext> : IPlugin
    {
        /// <summary>
        /// Lifetime of the plugin allowing to stop the plugin
        /// and check its state.
        /// </summary>
        public ILifetime Lifetime { get; }
        
        /// <summary>
        /// Used for initializing the module services
        /// </summary>
        /// <returns>Context of the plugin for registering responders</returns>
        public Task<TContext> InitAsync(TState state, CancellationToken token = new CancellationToken());

        /// <summary>
        /// Run should register the plugin to the application by assigning its handlers and starting its purpose 
        /// </summary>
        /// <remarks>
        /// WARNING: Run is expected not to block for long time.
        /// If the run operation is going to block, it is heavy, it should create a new thread.
        /// </remarks>
        /// <returns></returns>
        public Task RunAsync(CancellationToken token = new CancellationToken());

        /// <summary>
        /// Refresh should reload the configuration where needed.
        /// </summary>
        /// <returns></returns>
        public Task RefreshAsync(CancellationToken token = new CancellationToken());
    }
}