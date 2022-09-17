//
//   ApiApp.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Extensions;
using Christofel.Common.Database;
using Christofel.Helpers.ReadOnlyDatabase;
using Christofel.Logger;
using Christofel.Plugins.Lifetime;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Discord.Rest;
using Remora.Discord.Rest.Extensions;
using IApplicationLifetime = Christofel.Plugins.Lifetime.IApplicationLifetime;

namespace Christofel.Api
{
    /// <summary>
    /// Representation of standalone api application.
    /// </summary>
    public class ApiApp : IDisposable
    {
        private readonly PluginLifetimeHandler _lifetimeHandler;
        private IHost? _host;
        private IHostApplicationLifetime? _lifetime;
        private ILogger? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiApp"/> class.
        /// </summary>
        public ApiApp()
        {
            _lifetimeHandler = new PluginLifetimeHandler
            (
                e =>
                {
                    _logger?.LogError(e, "There was an error in the application, cannot recover");
                },
                () =>
                {
                    _lifetime?.StopApplication();
                }
            );
        }

        /// <summary>
        /// The lifetime of the application.
        /// </summary>
        public ICurrentPluginLifetime Lifetime => _lifetimeHandler.LifetimeSpecific;

        /// <inheritdoc />
        public void Dispose()
        {
            _lifetimeHandler.MoveToIfLower(LifetimeState.Stopping);
            _lifetimeHandler.MoveToIfLower(LifetimeState.Stopped);

            _host?.Dispose();

            _lifetimeHandler.MoveToIfLower(LifetimeState.Destroyed);
        }

        /// <summary>
        /// Run the application.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task RunAsync()
        {
            _lifetimeHandler.MoveToIfLower(LifetimeState.Starting);

            _lifetime?.ApplicationStarted.Register
            (
                () =>
                {
                    _lifetimeHandler.MoveToIfLower(LifetimeState.Running);
                }
            );

            _lifetime?.ApplicationStopping.Register
            (
                () =>
                {
                    _lifetimeHandler.MoveToIfLower(LifetimeState.Stopping);
                }
            );

            _lifetime?.ApplicationStopped.Register
            (
                () =>
                {
                    _lifetimeHandler.MoveToIfLower(LifetimeState.Stopped);
                }
            );

            _logger?.LogInformation("Hello world from ApiApp");
            await _host.RunAsync();
        }

        /// <summary>
        /// Inits the application.
        /// </summary>
        public void Init()
        {
            _lifetimeHandler.MoveToIfLower(LifetimeState.Initializing);

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("config.json")
                .Build();

            _host = CreateHostBuilder(Lifetime, configuration).Build();
            _lifetime = _host.Services.GetRequiredService<IHostApplicationLifetime>();
            _logger = _host.Services.GetRequiredService<ILogger<ApiApp>>();

            _lifetimeHandler.MoveToIfLower(LifetimeState.Initialized);
        }

        private static IHostBuilder CreateHostBuilder(ICurrentPluginLifetime lifetime, IConfiguration configuration)
            => Host.CreateDefaultBuilder()
                .ConfigureLogging
                (
                    builder =>
                    {
                        builder
                            .AddConfiguration(configuration.GetSection("Logging"))
                            .ClearProviders()
                            .AddFile()
                            .AddSimpleConsole(options => options.IncludeScopes = true)
                            .AddDiscordLogger();
                    }
                )
                .ConfigureServices
                (
                    services =>
                    {
                        services
                            .AddSingleton(lifetime)
                            .AddSingleton<IApplicationLifetime>(new ApplicationLifetimeWrapper(lifetime));

                        services
                            .AddDiscordRest(_ => (configuration.GetValue<string>("Bot:Token"), DiscordTokenType.Bot));

                        services
                            .AddSingleton<ReadonlyDbContextFactory<ChristofelBaseContext>>();

                        // Database
                        services
                            .AddChristofelDbContextFactory<ChristofelBaseContext>(configuration)
                            .AddTransient
                            (
                                p =>
                                    p.GetRequiredService<IDbContextFactory<ChristofelBaseContext>>().CreateDbContext()
                            );
                    }
                )
                .ConfigureWebHostDefaults
                (
                    webBuilder =>
                    {
                        webBuilder.UseStartup
                        (
                            _ => new Startup(configuration)
                        );

                        webBuilder.UseKestrel
                        (
                            kestrelOptions =>
                            {
                                kestrelOptions.ListenAnyIP(5000);
                            }
                        );
                    }
                );

        private class ApplicationLifetimeWrapper : IApplicationLifetime
        {
            private readonly ILifetime _lifetime;

            public ApplicationLifetimeWrapper(ILifetime lifetime)
            {
                _lifetime = lifetime;
            }

            public LifetimeState State => _lifetime.State;

            public bool IsErrored => _lifetime.IsErrored;

            public CancellationToken Errored => _lifetime.Errored;

            public CancellationToken Started => _lifetime.Started;

            public CancellationToken Stopped => _lifetime.Stopped;

            public CancellationToken Stopping => _lifetime.Stopping;

            public void RequestStop()
            {
                _lifetime.RequestStop();
            }
        }
    }
}