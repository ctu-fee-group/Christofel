using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Discord;
using Christofel.Api.OAuth;
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

                    services
                        .AddScoped<CtuOauthHandler>()
                        .AddScoped<DiscordOauthHandler>()
                        .Configure<OauthOptions>("Ctu", state.Configuration.GetSection("Oauth:Ctu"))
                        .Configure<OauthOptions>("Discord", state.Configuration.GetSection("Oauth:Discord"));

                    services
                        .AddScoped<DiscordApi>()
                        .Configure<DiscordApiOptions>(state.Configuration.GetSection("Apis:Discord"));

                    services
                        .AddCtuAuthProcess()
                })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

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

#if DEBUG
                    string? schemaPath = state.Configuration.GetValue<string?>("Debug:SchemaFile", null);
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

        public async Task RunAsync(CancellationToken token = new CancellationToken())
        {
            try
            {
                token.ThrowIfCancellationRequested();

                if (_lifetimeHandler.MoveToIfPrevious(LifetimeState.Starting) && !_lifetimeHandler.IsErrored)
                {
                    RegisterAspLifetime();

                    if (_host != null)
                    {
                        await _host.StartAsync(token);
                    }
                }
            }
            catch (Exception e)
            {
                _lifetimeHandler.MoveToError(e);
                throw;
            }
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
            Task.Run(async () =>
            {
                try
                {
                    _lifetimeHandler.MoveToIfLower(LifetimeState.Stopping);

                    //_aspLifetime?.StopApplication();

                    if (_host != null)
                    {
                        await _host.StopAsync();
                        _lifetimeHandler.MoveToIfLower(LifetimeState.Stopped);
                        _host.Dispose();
                    }

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
            });
        }
    }
}