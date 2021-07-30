using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
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
        
        protected abstract IEnumerable<IRefreshable> Refreshable { get; }
        protected abstract IEnumerable<IStoppable> Stoppable { get; }
        protected abstract IEnumerable<IStartable> Startable { get; }

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
        protected abstract Task InitializeServices(IServiceProvider services, CancellationToken token = new CancellationToken());

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
        protected virtual async Task DestroyServices(IServiceProvider? services, CancellationToken token = new CancellationToken())
        {
            token.ThrowIfCancellationRequested();
            if (services is ServiceProvider provider)
            {
                await provider.DisposeAsync();
            }
        }

        protected virtual Task InitAsync(CancellationToken token = new CancellationToken())
        {
            token.ThrowIfCancellationRequested();
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection = ConfigureServices(serviceCollection);

            Services = BuildServices(serviceCollection);

            return InitializeServices(Services, token);
        }

        public virtual Task InitAsync(IChristofelState state, CancellationToken token = new CancellationToken())
        {
            State = state;
            return InitAsync(token);
        }
        
        public virtual async Task RunAsync(CancellationToken token = new CancellationToken())
        {
            token.ThrowIfCancellationRequested();
            foreach (IStartable startable in Startable)
            {
                await startable.StartAsync(token);
            }
        }

        public virtual async Task StopAsync(CancellationToken token = new CancellationToken())
        {
            token.ThrowIfCancellationRequested();
            foreach (IStoppable stoppable in Stoppable)
            {
                await stoppable.StopAsync(token);
            }
        }

        public virtual async Task RefreshAsync(CancellationToken token = new CancellationToken())
        {
            token.ThrowIfCancellationRequested();
            foreach (IRefreshable refreshable in Refreshable)
            {
                await refreshable.RefreshAsync(token);
            }
        }

        public virtual Task DestroyAsync(CancellationToken token = new CancellationToken())
        {
            token.ThrowIfCancellationRequested();
            return DestroyServices(Services, token);
        }
    }
}