using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.BaseLib.Plugins
{
    /// <summary>
    /// Dependency injection plugin base class
    ///
    /// Contains DI using default Microsoft DI,
    /// can be used as base for plugins to start developing plugin faster
    /// </summary>
    public abstract class DIPlugin : IPlugin
    {
        private IServiceProvider? _services;
        private IChristofelState? _state;
        
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Version { get; }

        protected IChristofelState State
        {
            get
            {
                if (_state == null)
                {
                    throw new InvalidOperationException("State is null");
                }
                
                return _state;
            }
            set => _state = value;
        }
        
        protected IServiceProvider Services
        {
            get
            {
                if (_services == null)
                {
                    throw new InvalidOperationException("Services is null");
                }
                
                return _services;
            }
            set => _services = value;
        }

        /// <summary>
        /// Configure IServiceCollection to include common needed types
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <returns></returns>
        protected abstract IServiceCollection ConfigureServices(IServiceCollection serviceCollection);
        
        /// <summary>
        /// Initialize services that have some kind of state that needs to be addressed
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        protected abstract Task InitializeServices(IServiceProvider services);
        
        public abstract Task RunAsync();

        public abstract Task StopAsync();

        /// <summary>
        /// Build ServiceProvider itself from ServiceCollection
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <returns></returns>
        protected virtual IServiceProvider BuildServices(IServiceCollection serviceCollection)
        {
            return serviceCollection.BuildServiceProvider();
        }
        
        /// <summary>
        /// Dispose service provider disposing all IDisposable objects in it
        /// </summary>
        /// <param name="services"></param>
        protected virtual async Task DestroyServices(IServiceProvider? services)
        {
            if (services is ServiceProvider provider)
            {
                await provider.DisposeAsync();
            }
        }

        protected virtual Task InitAsync()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection = ConfigureServices(serviceCollection);

            Services = BuildServices(serviceCollection);

            return InitializeServices(Services);
        }

        public virtual Task InitAsync(IChristofelState state)
        {
            State = state;
            return InitAsync();
        }

        public virtual Task RefreshAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task DestroyAsync()
        {
            return DestroyServices(Services);
        }
    }
}