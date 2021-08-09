using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.Lifetime;
using Christofel.BaseLib.Plugins;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Christofel.Api
{
    public class ApiPlugin : IPlugin
    {
        private readonly PluginLifetimeHandler _lifetimeHandler;
        private ILogger<ApiPlugin>? _logger;
        private IHostApplicationLifetime? _aspLifetime;
        private IHost? _host;
        private Thread? _aspThread;

        public ApiPlugin()
        {
            _lifetimeHandler = new PluginLifetimeHandler(
                HandleError,
                HandleStop
            );
        }

        public string Name => "Christofel.Api";
        public string Description => "GraphQL API for Christofel";
        public string Version => "v0.0.1";
        public ILifetime Lifetime => _lifetimeHandler.Lifetime;

        private IHostBuilder CreateHostBuilder(IChristofelState state) =>
            Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddDiscordState(state)
                        .AddChristofelDatabase(state)
                        .Configure<BotOptions>(state.Configuration.GetSection("Bot"));
                })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

        public Task InitAsync(IChristofelState state, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                if (_lifetimeHandler.MoveToIfPrevious(LifetimeState.Initializing) && !_lifetimeHandler.IsErrored)
                { 
                    _host = CreateHostBuilder(state).Build();
                    _aspLifetime = _host.Services.GetRequiredService<IHostApplicationLifetime>();
                    _logger = _host.Services.GetRequiredService<ILogger<ApiPlugin>>();

                    _lifetimeHandler.MoveToState(LifetimeState.Initialized);
                }
            }
            catch (Exception e)
            {
                _lifetimeHandler.MoveToError(e);
                throw;
            }

            return Task.CompletedTask;
        }

        public Task RunAsync(CancellationToken token = new CancellationToken())
        {
            try
            {
                token.ThrowIfCancellationRequested();

                if (_lifetimeHandler.MoveToIfPrevious(LifetimeState.Starting) && !_lifetimeHandler.IsErrored)
                {
                    RegisterAspLifetime();
                    _aspThread = new Thread(() =>
                    {
                        try
                        {
                            _host.Run();
                        }
                        catch (Exception e)
                        {
                            _logger?.LogCritical(e, "Caught an exception inside ASP.NET thread");
                            _lifetimeHandler.RequestStop();
                        }
                    });
                    _aspThread.Start();
                }
            }
            catch (Exception e)
            {
                _lifetimeHandler.MoveToError(e);
                throw;
            }

            return Task.CompletedTask;
        }

        public Task RefreshAsync(CancellationToken token = new CancellationToken())
        {
            // Nothing to refresh for Api
            return Task.CompletedTask;
        }

        private void RegisterAspLifetime()
        {
            _aspLifetime?.ApplicationStarted.Register(() => { _lifetimeHandler.MoveToState(LifetimeState.Running); });

            _aspLifetime?.ApplicationStopping.Register(() => { _lifetimeHandler.RequestStop(); });

            _aspLifetime?.ApplicationStopped.Register(() => { _lifetimeHandler.RequestStop(); });
        }

        private void HandleError(Exception? e)
        {
            _logger?.LogError(e, $@"Plugin {Name} errored.");
            _lifetimeHandler.RequestStop();
        }

        private void HandleStop()
        {
            Task.Run(() =>
            {
                try
                {
                    _lifetimeHandler.MoveToIfLower(LifetimeState.Stopping);

                    _aspLifetime?.StopApplication();
                    _aspThread?.Join();
                    _lifetimeHandler.MoveToIfLower(LifetimeState.Stopped);

                    _host?.Dispose();

                    _aspThread = null;
                    _host = null;
                    _aspLifetime = null;
                    
                    _lifetimeHandler.MoveToState(LifetimeState.Destroyed);
                    _lifetimeHandler.Dispose();
                }
                catch (Exception e)
                {
                    _logger?.LogError(e, $"Plugin {Name} has thrown an exception during stopping");
                }
            });
        }
    }
}