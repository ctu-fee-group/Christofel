//
//   RuntimePluginService.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Plugins.Data;
using Christofel.Plugins.Extensions;
using Christofel.Plugins.Lifetime;
using Christofel.Plugins.Services;
using Microsoft.Extensions.Logging;

namespace Christofel.Plugins.Runtime
{
    public class RuntimePluginService<TState, TContext> : IPluginLifetimeService
    {
        private readonly ILogger _logger;
        private readonly PluginService _pluginService;
        protected TState State;

        public RuntimePluginService
            (TState state, PluginService pluginService, ILogger<RuntimePluginService<TState, TContext>> logger)
        {
            _pluginService = pluginService;
            _logger = logger;
            State = state;
        }

        public bool ShouldHandle(IPlugin plugin) => plugin is IRuntimePlugin<TState, TContext>;

        public Task<bool> InitializeAsync(AttachedPlugin plugin, CancellationToken token = default) =>
            InitializeAsync(GetState(plugin), plugin, token);

        public async Task<bool> DestroyAsync(AttachedPlugin plugin, CancellationToken token = default)
        {
            if (plugin.Plugin is not IRuntimePlugin<TState, TContext> runtimePlugin)
            {
                return false;
            }

            var lifetime = runtimePlugin.Lifetime;

            if (lifetime.State == LifetimeState.Destroyed)
            {
                return true;
            }

            _logger.LogInformation
                ($@"Requesting stop for plugin {plugin}, will wait at most 10 seconds for it to stop");
            try
            {
                lifetime.RequestStop();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Plugin {plugin} has thrown an exception during RequestStop call");
            }

            if (!await lifetime.WaitForAsync(LifetimeState.Stopped, 10000, token))
            {
                _logger.LogWarning($@"Plugin {plugin} does not respond to stop request, attaching for late destroy");
                lifetime.Stopped.Register
                (
                    () =>
                    {
                        if (plugin.DetachedPlugin is not null)
                        {
                            plugin.DetachedPlugin.DestroyedLate = true;
                            _logger.LogInformation($@"Plugin finally {plugin.DetachedPlugin} stopped late.");
                        }
                    }
                );
            }
            else if (!await lifetime.WaitForAsync(LifetimeState.Destroyed, 10000 / 5, token))
            {
                _logger.LogWarning($@"Plugin {plugin} stopped, but did not destroy itself in time. Lost it's track.");
            }
            else
            {
                _logger.LogInformation($@"Plugin {plugin} was stopped and destroyed in time given!");
                return true;
            }

            return false;
        }

        public async Task<bool> InitializeAsync(TState state, AttachedPlugin plugin, CancellationToken token = default)
        {
            if (plugin.Plugin is not IRuntimePlugin<TState, TContext> runtimePlugin)
            {
                return false;
            }

            await runtimePlugin.InitAsync(state, token);

            // Register lifetime callbacks
            var lifetime = runtimePlugin.Lifetime;
            RegisterLifetimeCallbacks(lifetime, plugin);

            if (token.IsCancellationRequested)
            {
                runtimePlugin.Lifetime.RequestStop();
                token.ThrowIfCancellationRequested();
            }

            await runtimePlugin.RunAsync(token);

            return true;
        }

        /// <summary>
        ///     Registers lifetime callbacks to allow plugin detach
        ///     without calling detach from the application
        /// </summary>
        /// <param name="lifetime"></param>
        /// <param name="plugin"></param>
        public void RegisterLifetimeCallbacks(ILifetime lifetime, AttachedPlugin plugin)
        {
            lifetime.Errored.Register
            (
                () =>
                {
                    Task.Run
                    (
                        () =>
                        {
                            try
                            {
                                _logger.LogError($@"Received error state from {plugin}. Going to detach it");

                                if (plugin.DetachedPlugin != null)
                                {
                                    _logger.LogInformation
                                    (
                                        $@"Plugin {plugin.DetachedPlugin} was already detached and this means that it was probably not well exited"
                                    );
                                }
                                else
                                {
                                    _pluginService.DetachAsync(plugin.Name)
                                        .GetAwaiter()
                                        .GetResult();
                                }
                            }
                            catch (Exception e)
                            {
                                _logger.LogCritical
                                (
                                    e,
                                    "Errored during Errored CancellationToken callback inside PluginLifetimeService"
                                );
                            }
                        }
                    );
                }
            );

            lifetime.Stopped.Register
            (
                () =>
                {
                    Task.Run
                    (
                        () =>
                        {
                            try
                            {
                                if (plugin.DetachedPlugin == null)
                                {
                                    _logger.LogWarning
                                        ($@"Plugin {plugin} reported stopped, but it was not requested, detaching it");
                                    try
                                    {
                                        _pluginService.DetachAsync(plugin.Name)
                                            .GetAwaiter()
                                            .GetResult();
                                    }
                                    catch (Exception e)
                                    {
                                        _logger.LogCritical(e, $"Plugin {plugin} errored during detaching");
                                    }
                                }
                                else
                                {
                                    _logger.LogInformation
                                    (
                                        $@"Plugin {plugin.DetachedPlugin} reported stopped and was detached/is being detached, not handling it in callback"
                                    );
                                }
                            }
                            catch (Exception e)
                            {
                                _logger.LogCritical
                                (
                                    e,
                                    "Errored during Stopped CancellationToken callback inside PluginLifetimeService"
                                );
                            }
                        }
                    );
                }
            );
        }

        protected virtual TState GetState(AttachedPlugin plugin) => State;
    }
}