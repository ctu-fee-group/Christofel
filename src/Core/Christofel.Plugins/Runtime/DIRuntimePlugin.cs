//
//  DIRuntimePlugin.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Plugins.Lifetime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Christofel.Plugins.Runtime
{
    /// <summary>
    /// Runtime plugin driven by Microsoft.Extensions.DependencyInjection.
    /// </summary>
    /// <remarks>
    /// Implements handling of lifetime for a plugin,
    /// allowing the user to implement
    /// how services should be created.
    /// </remarks>
    /// <typeparam name="TState">Shared state of the application.</typeparam>
    /// <typeparam name="TContext">Context of the plugin to be shared with the application.</typeparam>
    public abstract class DIRuntimePlugin<TState, TContext> : IRuntimePlugin<TState, TContext>
    {
        private TContext? _context;
        private IServiceProvider? _services;
        private TState? _state;

        /// <summary>
        /// Entities that will have <see cref="IRefreshable.RefreshAsync"/>> called on <see cref="RefreshAsync"/> call.
        /// </summary>
        protected abstract IEnumerable<IRefreshable> Refreshable { get; }

        /// <summary>
        /// Entities that will have <see cref="IStoppable.StopAsync"/>> called on <see cref="StopAsync"/> call.
        /// </summary>
        protected abstract IEnumerable<IStoppable> Stoppable { get; }

        /// <summary>
        /// Entities that will have <see cref="IStartable.StartAsync"/>> called on <see cref="RunAsync"/> call.
        /// </summary>
        protected abstract IEnumerable<IStartable> Startable { get; }

        /// <summary>
        /// Handler of the lifetime of this plugin.
        /// </summary>
        protected abstract LifetimeHandler LifetimeHandler { get; }

        /// <summary>
        /// State of the application that was given on <see cref="InitAsync"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Will be thrown if State is accessed before call to <see cref="InitAsync"/>.</exception>
        protected TState State
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

        /// <summary>
        /// Service provider for the plugin.
        /// </summary>
        /// <exception cref="InvalidOperationException">Will be thrown if Services is accessed before building the provider.</exception>
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

        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public abstract string Description { get; }

        /// <inheritdoc />
        public abstract string Version { get; }

        /// <inheritdoc />
        public ILifetime Lifetime => LifetimeHandler.Lifetime;

        /// <inheritdoc />
        public TContext Context
        {
            get
            {
                if (_context is null)
                {
                    throw new InvalidOperationException("Context is null");
                }

                return _context;
            }
            private set => _context = value;
        }

        /// <inheritdoc />
        public virtual Task InitAsync(TState state, CancellationToken token = default)
        {
            State = state;
            return InitAsync(token);
        }

        /// <inheritdoc />
        public virtual Task RunAsync(CancellationToken token = default) =>
            RunAsync(true, Startable, token);

        /// <inheritdoc />
        public virtual async Task RefreshAsync(CancellationToken token = default)
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
        /// Should create the initialized <see cref="TContext"/> of the plugin.
        /// </summary>
        /// <returns>Initialized context of the plugin.</returns>
        protected abstract TContext InitializeContext();

        /// <summary>
        /// Used for configuration of the service collection before building the provider.
        /// </summary>
        /// <param name="serviceCollection">Collection to be configured.</param>
        /// <returns>The collection to be built into service provider.</returns>
        protected abstract IServiceCollection ConfigureServices(IServiceCollection serviceCollection);

        /// <summary>
        /// Initializes services after provider is built if any of the services need custom initialization.
        /// </summary>
        /// <param name="services">Built provider with the services.</param>
        /// <param name="token">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        protected abstract Task InitializeServices
            (IServiceProvider services, CancellationToken token = default);

        /// <summary>
        /// Builds the service collection.
        /// </summary>
        /// <param name="serviceCollection">Collection to be built.</param>
        /// <returns>Built provider of the services.</returns>
        protected virtual IServiceProvider BuildServices
            (IServiceCollection serviceCollection) => serviceCollection.BuildServiceProvider();

        /// <summary>
        /// Destroys all <see cref="IDisposable"/> services that are held by this plugin.
        /// </summary>
        /// <param name="services">The service provider used to hold all of the services.</param>
        /// <param name="token">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        protected virtual async Task DestroyServices
            (IServiceProvider? services, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (services is ServiceProvider provider)
            {
                await provider.DisposeAsync();
            }

            LifetimeHandler.Dispose();
        }

        /// <summary>
        /// Initializes the service provider returning context of the plugin.
        /// </summary>
        /// <param name="token">The cancellation token for this operation.</param>
        /// <returns>Context of the plugin.</returns>
        protected virtual async Task<TContext> InitAsync(CancellationToken token = default)
        {
            if (_context is null)
            {
                _context = InitializeContext();
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
        /// Calls all <see cref="startables"/> objects.
        /// </summary>
        /// <param name="handleLifetime">Whether the lifetime should be handled (set to Starting at the start, Started at the end) by this method.</param>
        /// <param name="startables">What startables should be started.</param>
        /// <param name="token">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        protected virtual async Task RunAsync
            (bool handleLifetime, IEnumerable<IStartable> startables, CancellationToken token = default)
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
        /// Calls <see cref="Stoppable"/> objets.
        /// </summary>
        /// <param name="token">The cancellation token for this operation.</param>
        /// <exception cref="AggregateException">Thrown if there were any errors while stopping. All of the erros will be grouped.</exception>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public virtual async Task StopAsync(CancellationToken token = default)
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
        /// Destroys disposable services that are held by this plugin.
        /// </summary>
        /// <param name="token">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public virtual async Task DestroyAsync(CancellationToken token = default)
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
        /// Called on end of <see cref="InitAsync"/>
        /// to do internal init.
        /// </summary>
        /// <param name="token">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        protected virtual Task InternalInitAsync(CancellationToken token = default) => Task.CompletedTask;

        /// <summary>
        /// Called on end of <see cref="RunAsync"/>
        /// to start internal features.
        /// </summary>
        /// <param name="token">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        protected virtual Task InternalRunAsync(CancellationToken token = default) => Task.CompletedTask;

        /// <summary>
        /// Called on end of <see cref="RefreshAsync"/>
        /// to refresh internal features.
        /// </summary>
        /// <param name="token">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        protected virtual Task InternalRefreshAsync(CancellationToken token = default) => Task.CompletedTask;

        /// <summary>
        /// Called on end of <see cref="StopAsync"/>
        /// to stop internal features.
        /// </summary>
        /// <param name="token">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        protected virtual Task InternalStopAsync(CancellationToken token = default) => Task.CompletedTask;

        /// <summary>
        /// Called on end of <see cref="DestroyAsync"/>
        /// to destroy internal features.
        /// </summary>
        /// <param name="token">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        protected virtual Task InternalDestroyAsync(CancellationToken token = default) => Task.CompletedTask;

        /// <summary>
        /// Creates default handler of a stop request calling StopAsync and DestroyAsync.
        /// </summary>
        /// <param name="requestLogger">Function to retrieve the logger of the plugin.</param>
        /// <returns>An action that handles the request for a stop of the plugin.</returns>
        protected Action DefaultHandleStopRequest(Func<ILogger?> requestLogger)
        {
            return () =>
            {
                var logger = requestLogger();
                Task.Run
                (
                    async () =>
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
                    }
                );
            };
        }

        /// <summary>
        /// Creates default handler of an error logging the error and requesting a stop.
        /// </summary>
        /// <param name="requestLogger">Function to retrieve the logger of the plugin.</param>
        /// <returns>An action that handles the error state of the plugin.</returns>
        protected Action<Exception?> DefaultHandleError(Func<ILogger?> requestLogger)
        {
            return e =>
            {
                var logger = requestLogger();
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