using System.Threading;
using System.Threading.Tasks;

namespace Christofel.BaseLib.Plugins
{
    /// <summary>
    /// Interface for implementing plugin
    ///
    /// Plugins are attached during runtime to allow changing parts of the application in runtime
    ///
    /// Plugin lifetime goes from uninitialized, initialized (InitAsync), running (RunAsync), stopped (StopAsync), destroyed (DestroyAsync).
    /// Separation of Init/Run and Stop/Destroy is not needed, but chosen to better separate what happens where
    /// </summary>
    public interface IPlugin : IHasPluginInfo
    {
        /// <summary>
        /// Used for initializing the module services
        /// </summary>
        /// <param name="state">Shared application state</param>
        /// <returns></returns>
        public Task InitAsync(IChristofelState state, CancellationToken token = new CancellationToken());

        /// <summary>
        /// Run should register the plugin to the application by assigning its handlers and starting its purpose 
        /// 
        /// WARNING: Run is expected not to block for long time.
        /// If the run operation is going to block, it is heavy, it should create a new thread.
        /// </summary>
        /// <returns></returns>
        public Task RunAsync(CancellationToken token = new CancellationToken());
        
        /// <summary>
        /// Should stop the running operation.
        /// The plugin should stop handling any operations associated with it and be prepared for destroy.
        /// </summary>
        /// <returns></returns>
        public Task StopAsync(CancellationToken token = new CancellationToken());

        /// <summary>
        /// Refresh should reload the configuration where needed.
        /// </summary>
        /// <returns></returns>
        public Task RefreshAsync(CancellationToken token = new CancellationToken());
        
        /// <summary>
        /// Destroys all the services, the plugin is unusable after destroying
        /// </summary>
        /// <returns></returns>
        public Task DestroyAsync(CancellationToken token = new CancellationToken());
    }
}