using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Lifetime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

        protected abstract LifetimeHandler LifetimeHandler { get; }
        public ILifetime Lifetime => LifetimeHandler.Lifetime;

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

        protected virtual async Task InitAsync(CancellationToken token = new CancellationToken())
        {
            if (!VerifyStateAndIncrement(LifetimeState.Startup))
            {
                return;
            }

            try
            {
                token.ThrowIfCancellationRequested();

                IServiceCollection serviceCollection = new ServiceCollection();
                serviceCollection = ConfigureServices(serviceCollection);
                
                Services = BuildServices(serviceCollection);

                await InitializeServices(Services, token);
            }
            catch (Exception e)
            {
                LifetimeHandler.MoveToError(e);
                throw;
            }

            VerifyStateAndIncrement(LifetimeState.Initializing);
        }
        
        /// <summary>
        /// Initializes services
        /// </summary>
        /// <param name="state"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual Task InitAsync(IChristofelState state, CancellationToken token = new CancellationToken())
        {
            State = state;
            return InitAsync(token);
        }
        
        /// <summary>
        /// Runs needed services from Startable
        /// </summary>
        /// <param name="token"></param>
        public virtual async Task RunAsync(CancellationToken token = new CancellationToken())
        {
            if (!VerifyStateAndIncrement(LifetimeState.Initialized))
            {
                return;
            }

            token.ThrowIfCancellationRequested();

            try
            {
                foreach (IStartable startable in Startable)
                {
                    await startable.StartAsync(token);
                }
            }
            catch (Exception e)
            {
                LifetimeHandler.MoveToError(e);
                throw;
            }

            VerifyStateAndIncrement(LifetimeState.Starting);
        }

        /// <summary>
        /// Stops needed services from Stoppable
        /// </summary>
        /// <param name="token"></param>
        /// <exception cref="AggregateException"></exception>
        public virtual async Task StopAsync(CancellationToken token = new CancellationToken())
        {
            if (!VerifyStateAndIncrement(LifetimeState.Running) && !LifetimeHandler.IsErrored)
            {
                return;
            }
            
            token.ThrowIfCancellationRequested();
            List<Exception> exceptions = new List<Exception>();
            foreach (IStoppable stoppable in Stoppable)
            {
                try
                {
                    await stoppable.StopAsync(token);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    LifetimeHandler.MoveToError(e);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }

            VerifyStateAndIncrement(LifetimeState.Stopping);
        }

        /// <summary>
        /// Refreshes needed services from Refreshable
        /// </summary>
        /// <param name="token"></param>
        public virtual async Task RefreshAsync(CancellationToken token = new CancellationToken())
        {
            if (LifetimeHandler.Lifetime.State != LifetimeState.Running)
            {
                return;
            }

            token.ThrowIfCancellationRequested();

            try
            {
                foreach (IRefreshable refreshable in Refreshable)
                {
                    await refreshable.RefreshAsync(token);
                }
            }
            catch (Exception e)
            {
                LifetimeHandler.MoveToError(e);
                throw;
            }
        }

        /// <summary>
        /// Destroys and disposes all the services
        /// </summary>
        /// <param name="token"></param>
        public virtual async Task DestroyAsync(CancellationToken token = new CancellationToken())
        {
            if (VerifyStateAndIncrement(LifetimeState.Stopped) && !LifetimeHandler.IsErrored)
            {
                return;
            }
            
            token.ThrowIfCancellationRequested();
            try
            {
                await DestroyServices(Services, token);
            }
            catch (Exception e)
            {
                LifetimeHandler.MoveToError(e);
                throw;
            }
        }

        /// <summary>
        /// Verifies if the state is expected,
        /// if it is, increments it and returns true
        ///
        /// If it isn't, just return false
        /// </summary>
        /// <param name="expected"></param>
        /// <returns></returns>
        protected virtual bool VerifyStateAndIncrement(LifetimeState expected)
        {
            if (LifetimeHandler.Lifetime.State != expected)
            {
                return false;
            }

            LifetimeHandler.NextState();
            return true;
        }

        /// <summary>
        /// Default handler of a stop request calling StopAsync and DestroyAsync
        /// </summary>
        /// <param name="requestLogger"></param>
        /// <returns></returns>
        protected Action DefaultHandleStopRequest(Func<ILogger?> requestLogger)
        {
            return () =>
            {
                ILogger? logger = requestLogger();
                Task.Run(async () =>
                {
                    try
                    {
                        await StopAsync();
                    }
                    catch (Exception e)
                    {
                        logger?.LogError(e, $@"{Name} plugin errored during stop, ignoring the exception");
                    }
                    
                    try
                    {
                        await DestroyAsync();
                    }
                    catch (Exception e)
                    {
                        logger?.LogError(e, $@"{Name} plugin errored during destroying, ignoring the exception");
                    }
                });
            };
        }

        /// <summary>
        /// Default handler of errored state Requesting stop and logging its state usign logger
        /// </summary>
        /// <param name="requestLogger"></param>
        /// <returns></returns>
        protected Action<Exception?> DefaultHandleError(Func<ILogger?> requestLogger)
        {
            return (e) =>
            {
                ILogger? logger = requestLogger();
                try
                {
                    LifetimeHandler.RequestStop();
                    logger.LogCritical(e, $@"{Name} plugin errored, trying to stop");
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, $@"{Name} plugin errored during handling errored state, whoops");
                }
            };
        }
    }
}