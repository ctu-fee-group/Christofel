using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Implementations.ReadOnlyDatabase;
using Christofel.BaseLib.Lifetime;
using Christofel.BaseLib.Plugins;
using Christofel.Logger;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Discord.Rest.Extensions;
using IApplicationLifetime = Christofel.BaseLib.Plugins.IApplicationLifetime;

namespace Christofel.Api
{
    public class ApiApp : IDisposable
    {
        private readonly PluginLifetimeHandler _lifetimeHandler;
        private IHost? _host;
        private IHostApplicationLifetime? _lifetime;
        private ILogger? _logger;

        public ApiApp()
        {
            _lifetimeHandler = new PluginLifetimeHandler(
                e => { _logger?.LogError(e, "There was an error in the application, cannot recover"); },
                () => { _lifetime?.StopApplication(); });
        }

        public ICurrentPluginLifetime Lifetime => _lifetimeHandler.LifetimeSpecific;

        public async Task RunAsync()
        {
            _lifetimeHandler.MoveToIfLower(LifetimeState.Starting);

            _lifetime?.ApplicationStarted.Register(() =>
            {
                _lifetimeHandler.MoveToIfLower(LifetimeState.Running);
            });
            
            _lifetime?.ApplicationStopping.Register(() =>
            {
                _lifetimeHandler.MoveToIfLower(LifetimeState.Stopping);
            });

            _lifetime?.ApplicationStopped.Register(() =>
            {
                _lifetimeHandler.MoveToIfLower(LifetimeState.Stopped);
            });

            _logger.LogInformation("Hello world from ApiApp");
            await _host.RunAsync();
        }

        public void Dispose()
        {
            _lifetimeHandler.MoveToIfLower(LifetimeState.Stopping);
            _lifetimeHandler.MoveToIfLower(LifetimeState.Stopped);
            
            _host?.Dispose();

            _lifetimeHandler.MoveToIfLower(LifetimeState.Destroyed);
        }

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

        private static IHostBuilder CreateHostBuilder(ICurrentPluginLifetime lifetime, IConfiguration configuration) =>
            Host.CreateDefaultBuilder()
                .ConfigureLogging(builder =>
                {
                    builder
                        .AddConfiguration(configuration.GetSection("Logging"))
                        .ClearProviders()
                        .AddFile()
                        .AddSimpleConsole(options => options.IncludeScopes = true)
                        .AddDiscordLogger();
                })
                .ConfigureServices(services =>
                {
                    services
                        .AddSingleton<ICurrentPluginLifetime>(lifetime)
                        .AddSingleton<IApplicationLifetime>(new ApplicationLifetimeWrapper(lifetime));

                    services
                        .AddDiscordRest(_ => configuration.GetValue<string>("Bot:Token"));

                    services
                        .AddSingleton<ReadonlyDbContextFactory<ChristofelBaseContext>>();
                    
                    services
                        // db
                        .AddDbContextFactory<ChristofelBaseContext>(options =>
                            options
                                .UseMySql(
                                    configuration.GetConnectionString("ChristofelBase"),
                                    ServerVersion.AutoDetect(configuration.GetConnectionString("ChristofelBase")
                                    ))
                        )
                        .AddTransient<ChristofelBaseContext>(p =>
                            p.GetRequiredService<IDbContextFactory<ChristofelBaseContext>>().CreateDbContext());
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseConfiguration(configuration);
                    webBuilder.UseStartup<Startup>();
                });

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