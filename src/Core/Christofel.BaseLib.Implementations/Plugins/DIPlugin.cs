using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Lifetime;
using Christofel.Plugins;
using Christofel.Plugins.Lifetime;
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
    public abstract class DIPlugin : IChristofelRuntimePlugin
    {
        private IServiceProvider? _services;
        private IChristofelState? _state;
        private PluginContext? _context;
        
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Version { get; }
        
        protected abstract IEnumerable<IRefreshable> Refreshable { get; }
        protected abstract IEnumerable<IStoppable> Stoppable { get; }
        protected abstract IEnumerable<IStartable> Startable { get; }

        protected abstract LifetimeHandler LifetimeHandler { get; }
        public ILifetime Lifetime => LifetimeHandler.Lifetime;

        IPluginContext IRuntimePlugin<IChristofelState, IPluginContext>.Context => Context;

        protected PluginContext Context
        {
            get
            {
                if (_context is null)
                {
                    throw new InvalidOperationException("Context is null");
                }

                return _context;
            }
            set => _context = value;
        }

        protected IChristofelState State
        {
            get
            {
                if (_state is null)
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
            
            LifetimeHandler.Dispose();
        }

        protected virtual async Task<IPluginContext> InitAsync(CancellationToken token = new CancellationToken())
        {
            if (_context is null)
            {
                _context = new PluginContext();
            }
            
            if (!LifetimeHandler.MoveToIfPrevious(LifetimeState.Initializing))
            {
                return Context;
            }

            try
            {
                token.ThrowIfCancellationRequested();

                IServiceCollection serviceCollection = new ServiceCollection();
                serviceCollection = ConfigureServices(serviceCollection);
                
                Services = BuildServices(serviceCollection);

                await InitializeServices(Services, token);
                await InternalInitAsync(token);
            }
            catch (Exception e)
            {
                LifetimeHandler.MoveToError(e);
                throw;
            }

            LifetimeHandler.MoveToIfPrevious(LifetimeState.Initialized);
            return Context;
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
        public virtual  Task RunAsync(CancellationToken token = new CancellationToken())
        {
            return RunAsync(true, Startable, token);
        }

        /// <summary>
        /// Runs needed services from Startable
        /// </summary>
        /// <param name="startables"></param>
        /// <param name="token"></param>
        /// <param name="handleLifetime"></param>
        protected virtual async Task RunAsync(bool handleLifetime, IEnumerable<IStartable> startables, CancellationToken token = new CancellationToken())
        {
            if (handleLifetime)
            {
                if (!LifetimeHandler.MoveToIfPrevious(LifetimeState.Starting))
                {
                    return;
                }
            }

            token.ThrowIfCancellationRequested();

            try
            {
                foreach (IStartable startable in startables)
                {
                    token.ThrowIfCancellationRequested();
                    await startable.StartAsync(token);
                }
                
                await InternalRunAsync(token);
            }
            catch (Exception e)
            {
                LifetimeHandler.MoveToError(e);
                throw;
            }

            if (handleLifetime)
            {
                LifetimeHandler.MoveToIfPrevious(LifetimeState.Running);
            }
        }
        
        /// <summary>
        /// Stops needed services from Stoppable
        /// </summary>
        /// <param name="token"></param>
        /// <exception cref="AggregateException"></exception>
        public virtual async Task StopAsync(CancellationToken token = new CancellationToken())
        {
            if (!LifetimeHandler.MoveToIfPrevious(LifetimeState.Stopping) && !LifetimeHandler.IsErrored)
            {
                return;
            }
            
            LifetimeHandler.MoveToIfLower(LifetimeState.Stopping);
            
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
            
            try
            {
                await InternalStopAsync(token);
            }
            catch (Exception e)
            {
                exceptions.Add(e);
                LifetimeHandler.MoveToError(e);
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }

            LifetimeHandler.MoveToIfLower(LifetimeState.Stopped);
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
                await InternalRefreshAsync(token);
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
            token.ThrowIfCancellationRequested();
            LifetimeHandler.MoveToIfLower(LifetimeState.Stopping);
            LifetimeHandler.MoveToIfLower(LifetimeState.Stopped);
            
            try
            {
                await DestroyServices(Services, token);
                await InternalDestroyAsync(token);
            }
            catch (Exception e)
            {
                LifetimeHandler.MoveToError(e);
                throw;
            }
            LifetimeHandler.MoveToIfLower(LifetimeState.Destroyed);
        }

        /// <summary>
        /// Called on end of init
        /// to do internal init
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected virtual Task InternalInitAsync(CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called on end of starting
        /// to start internal features
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected virtual Task InternalRunAsync(CancellationToken token = default)
        {
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Called on end of refreshing
        /// to refresh internal features
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected virtual Task InternalRefreshAsync(CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called on end of stopping
        /// to stop internal features
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected virtual Task InternalStopAsync(CancellationToken token = default)
        {
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Called on end of destroyal
        /// to destroy internal features
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected virtual Task InternalDestroyAsync(CancellationToken token = default)
        {
            return Task.CompletedTask;
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