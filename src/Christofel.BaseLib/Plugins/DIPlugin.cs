using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.BaseLib.Plugins
{
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

        protected abstract IServiceCollection ConfigureServices(IServiceCollection serviceCollection);
        protected abstract Task InitializeServices(IServiceProvider services);

        protected virtual IServiceProvider BuildServices(IServiceCollection serviceCollection)
        {
            return serviceCollection.BuildServiceProvider();
        }
        
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