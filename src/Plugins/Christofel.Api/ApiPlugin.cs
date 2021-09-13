//
//   ApiPlugin.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.Plugins;
using Christofel.Common;
using Christofel.Plugins;
using Christofel.Plugins.Lifetime;
using Christofel.Remora;
using Christofel.Scheduler.Abstractions;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Christofel.Api
{
    /// <summary>
    /// Plugin representing GraphQL api.
    /// </summary>
    public class ApiPlugin : IRuntimePlugin<IChristofelState, PluginContext>
    {
        private readonly PluginLifetimeHandler _lifetimeHandler;
        private IHostApplicationLifetime? _aspLifetime;
        private Thread? _aspThread;
        private IHost? _host;
        private ILogger<ApiPlugin>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiPlugin"/> class.
        /// </summary>
        public ApiPlugin()
        {
            Context = new PluginContext();
            _lifetimeHandler = new PluginLifetimeHandler
            (
                HandleError,
                HandleStop
            );
        }

        /// <inheritdoc />
        public string Name => "Christofel.Api";

        /// <inheritdoc />
        public string Description => "GraphQL API for Christofel";

        /// <inheritdoc />
        public string Version => "v0.0.1";

        /// <inheritdoc />
        public ILifetime Lifetime => _lifetimeHandler.Lifetime;

        /// <inheritdoc />
        public PluginContext Context { get; }

        /// <inheritdoc />
        public async Task InitAsync(IChristofelState state, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                if (_lifetimeHandler.MoveToIfPrevious(LifetimeState.Initializing) && !_lifetimeHandler.IsErrored)
                {
                    _host = CreateHostBuilder(state).Build();
                    _aspLifetime = _host.Services.GetRequiredService<IHostApplicationLifetime>();
                    _logger = _host.Services.GetRequiredService<ILogger<ApiPlugin>>();
                    Context.SchedulerJobExecutor = _host.Services.GetRequiredService<IJobExecutor>();
                    Context.SchedulerJobStore = _host.Services.GetRequiredService<IJobStore>();

#if DEBUG
                    var schemaPath = state.Configuration.GetValue<string?>("Debug:SchemaFile", null);
                    if (schemaPath != null)
                    {
                        IRequestExecutor executor = await _host.Services.GetRequiredService<IRequestExecutorResolver>()
                            .GetRequestExecutorAsync(default, token);
                        await File.WriteAllTextAsync(schemaPath, executor.Schema.Print(), token);
                    }
#endif
                    _lifetimeHandler.MoveToState(LifetimeState.Initialized);
                }
            }
            catch (Exception e)
            {
                _lifetimeHandler.MoveToError(e);
                throw;
            }
        }

        /// <inheritdoc />
        public Task RunAsync(CancellationToken token = default)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                if (_lifetimeHandler.MoveToIfPrevious(LifetimeState.Starting) && !_lifetimeHandler.IsErrored)
                {
                    RegisterAspLifetime();

                    if (_host != null)
                    {
                        _aspThread = new Thread(_host.Run);
                        _aspThread.Start();
                    }
                }
            }
            catch (Exception e)
            {
                _lifetimeHandler.MoveToError(e);
                throw;
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task RefreshAsync(CancellationToken token = default) =>
            Task.CompletedTask; // Nothing to refresh for Api

        private IHostBuilder CreateHostBuilder(IChristofelState state) =>
            Host.CreateDefaultBuilder()
                .ConfigureServices
                (
                    services =>
                    {
                        services
                            .AddSingleton(_lifetimeHandler.LifetimeSpecific);

                        services
                            .AddDiscordState(state)
                            .AddChristofelDatabase(state);
                    }
                )
                .ConfigureWebHostDefaults
                (
                    webBuilder =>
                    {
                        webBuilder.UseConfiguration(state.Configuration);
                        webBuilder.UseStartup<Startup>();
                    }
                );

        private void RegisterAspLifetime()
        {
            _aspLifetime?.ApplicationStarted.Register
            (
                () =>
                {
                    _lifetimeHandler.MoveToState(LifetimeState.Running);
                }
            );

            _aspLifetime?.ApplicationStopping.Register
            (
                () =>
                {
                    _lifetimeHandler.RequestStop();
                }
            );

            _aspLifetime?.ApplicationStopped.Register
            (
                () =>
                {
                    _lifetimeHandler.RequestStop();
                }
            );
        }

        private void HandleError(Exception? e)
        {
            _logger?.LogError(e, $@"Plugin {Name} errored.");
            _lifetimeHandler.RequestStop();
        }

        private void HandleStop()
        {
            Task.Run
            (
                () =>
                {
                    try
                    {
                        _lifetimeHandler.MoveToIfLower(LifetimeState.Stopping);

                        _aspLifetime?.StopApplication();
                        _aspThread?.Join();
                        _lifetimeHandler.MoveToIfLower(LifetimeState.Stopped);

                        _host?.Dispose();

                        _lifetimeHandler.MoveToIfLower(LifetimeState.Stopped);

                        _host = null;
                        _aspLifetime = null;

                        _lifetimeHandler.MoveToState(LifetimeState.Destroyed);
                        _lifetimeHandler.Dispose();
                    }
                    catch (Exception e)
                    {
                        _logger?.LogError(e, $"Plugin {Name} has thrown an exception during stopping");
                    }
                }
            );
        }
    }
}