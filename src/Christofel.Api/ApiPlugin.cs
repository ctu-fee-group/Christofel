using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu;
using Christofel.Api.Ctu.Extensions;
using Christofel.Api.Ctu.Steps;
using Christofel.Api.Ctu.Steps.Roles;
using Christofel.Api.Discord;
using Christofel.Api.OAuth;
using Christofel.BaseLib;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.Lifetime;
using Christofel.BaseLib.Plugins;
using HotChocolate.Execution;
using Kos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Usermap;

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
                        .AddSingleton<ICurrentPluginLifetime>(_lifetimeHandler.LifetimeSpecific);
                    
                    services
                        .AddDiscordState(state)
                        .AddChristofelDatabase(state)
                        .Configure<BotOptions>(state.Configuration.GetSection("Bot"));

                    services
                        .AddScoped<CtuOauthHandler>()
                        .AddScoped<DiscordOauthHandler>()
                        .Configure<CtuOauthOptions>("Ctu", state.Configuration.GetSection("Oauth:Ctu"))
                        .Configure<OauthOptions>("Discord", state.Configuration.GetSection("Oauth:Discord"));

                    services
                        .AddScoped<DiscordApi>()
                        .Configure<DiscordApiOptions>(state.Configuration.GetSection("Apis:Discord"));

                    services
                        .AddScoped<UsermapApi>()
                        .Configure<UsermapApiOptions>(state.Configuration.GetSection("Apis:Usermap"))
                        .AddScoped<KosApi>()
                        .Configure<KosApiOptions>(state.Configuration.GetSection("Apis:Kos"));

                    services
                        .AddSingleton<CtuAuthRoleAssignProcessor>();
                    
                    services
                        .AddCtuAuthProcess()
                        .AddCtuAuthStep<VerifyCtuUsernameStep>() // If ctu username is set and new auth user does not match, error
                        .AddCtuAuthStep<VerifyDuplicityStep>() // Handle duplicate
                        .AddCtuAuthStep<SpecificRolesStep>() // Add specific roles
                        .AddCtuAuthStep<UsermapRolesStep>() // Add usermap roles
                        .AddCtuAuthStep<TitlesRoleStep>() // Add roles based on title rules
                        .AddCtuAuthStep<ProgrammeRoleStep>() // Obtain programme and its roles
                        .AddCtuAuthStep<YearRoleStep>() // Obtain study start year and its roles
                        .AddCtuAuthStep<RemoveOldRolesStep>() // Remove all assigned roles by the auth process that weren't added by the previous steps
                        .AddCtuAuthStep<AssignRolesStep>() // Assign roles to the user in queue
                        .AddCtuAuthStep<FinishVerificationStep>();
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
            });
        }
    }
}