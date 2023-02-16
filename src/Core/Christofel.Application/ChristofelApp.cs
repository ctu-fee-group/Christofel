//
//   ChristofelApp.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Application.Commands;
using Christofel.Application.Permissions;
using Christofel.Application.Plugins;
using Christofel.Application.Responders;
using Christofel.Application.State;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.Plugins;
using Christofel.CommandsLib;
using Christofel.CommandsLib.Extensions;
using Christofel.Common;
using Christofel.Common.Database;
using Christofel.Common.Discord;
using Christofel.Common.Permissions;
using Christofel.Helpers;
using Christofel.Helpers.ReadOnlyDatabase;
using Christofel.Logger;
using Christofel.Plugins;
using Christofel.Plugins.Lifetime;
using Christofel.Plugins.Runtime;
using Christofel.Remora;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Commands.Extensions;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Extensions;
using Remora.Results;

namespace Christofel.Application
{
    /// <summary>
    /// Delegate for refreshing christofel application.
    /// </summary>
    /// <param name="token">The cancellation token for the operation.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    public delegate Task RefreshChristofel(CancellationToken token);

    /// <summary>
    /// Christofel application managing state of the application.
    /// </summary>
    public class ChristofelApp : ChristofelDIPlugin, IRefreshable, IStoppable
    {
        private readonly IConfigurationRoot _configuration;
        private readonly ChristofelLifetimeHandler _lifetimeHandler;
        private ILogger<ChristofelApp>? _logger;
        private bool _running;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChristofelApp"/> class.
        /// </summary>
        /// <param name="commandArgs">The arguments from the command line.</param>
        public ChristofelApp(string[] commandArgs)
        {
            _lifetimeHandler = new ChristofelLifetimeHandler(DefaultHandleError(() => _logger), this);
            _configuration = CreateConfiguration(commandArgs);
        }

        /// <inheritdoc/>
        public override string Name => "Christofel.Application";

        /// <inheritdoc/>
        public override string Description => "Base application with module commands managing their lifecycle";

        /// <inheritdoc />
        public override string Version => "v0.0.1";

        /// <inheritdoc />
        protected override IEnumerable<IStartable> Startable => Enumerable.Empty<IStartable>();

        /// <summary>
        /// Start after Ready event was received.
        /// </summary>
        private IEnumerable<IStartable> DeferStartable
        {
            get
            {
                yield return Services.GetRequiredService<PluginAutoloader>();
                yield return Services.GetRequiredService<ChristofelCommandRegistrator>();
            }
        }

        /// <inheritdoc />
        protected override LifetimeHandler LifetimeHandler => _lifetimeHandler;

        /// <inheritdoc />
        Task IRefreshable.RefreshAsync(CancellationToken token)
        {
            _logger?.LogInformation("Refreshing Christofel");
            _configuration.Reload();
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        Task IStoppable.StopAsync(CancellationToken token)
        {
            _logger?.LogInformation("Stopping Christofel");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Creates configuration of the application from json files, environment variables and command line args.
        /// </summary>
        /// <param name="commandArgs">The command line arguments.</param>
        /// <returns>The built configuration.</returns>
        public static IConfigurationRoot CreateConfiguration(string[] commandArgs)
        {
            string environment = Environment.GetEnvironmentVariable("ENV") ?? "production";

            return new ConfigurationBuilder()
                .AddJsonFile(Environment.GetEnvironmentVariable("CHRISTOFEL_CONFIG_PATH") ?? "config.json", false, true)
                .AddJsonFile(Environment.GetEnvironmentVariable("CHRISTOFEL_ENVIRONMENT_CONFIG_PATH") ?? $@"config.{environment}.json", true, true)
                .AddEnvironmentVariables()
                .AddCommandLine(commandArgs)
                .Build();
        }

        /// <inheritdoc />
        protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<IChristofelState, ChristofelState>()
                .AddSingleton(_lifetimeHandler.LifetimeSpecific)

                // plugins
                .AddPlugins()
                .AddSingleton<IResultLoggerProvider, ResultLoggerProvider>()
                .AddStateful<PluginAutoloader>(ServiceLifetime.Transient)
                .AddRuntimePlugins<IChristofelState, IPluginContext>()

                // config
                .AddSingleton<IConfiguration>(_configuration)
                .Configure<BotOptions>(_configuration.GetSection("Bot"))
                .Configure<DiscordBotOptions>(_configuration.GetSection("Bot"))
                .Configure<PluginServiceOptions>(_configuration.GetSection("Plugins"))
                .Configure<PluginAutoloaderOptions>(_configuration.GetSection("Plugins"))
                .AddSingleton<IBot, DiscordBot>()

                // db
                .AddChristofelDbContextFactory<ChristofelBaseContext>(_configuration)
                .AddScoped
                (
                    p =>
                        p.GetRequiredService<IDbContextFactory<ChristofelBaseContext>>().CreateDbContext()
                )
                .AddSingleton<ReadonlyDbContextFactory<ChristofelBaseContext>>()

                // permissions
                .AddSingleton<IPermissionService, ListPermissionService>()
                .AddTransient<IPermissionsResolver, DbPermissionsResolver>()

                // loggings
                .AddLogging
                (
                    builder =>
                    {
                        builder
                            .AddConfiguration(_configuration.GetSection("Logging"))
                            .ClearProviders()
                            .AddFile()
                            .AddSimpleConsole(options => options.IncludeScopes = true)
                            .AddDiscordLogger();
                    }
                )

                // discord
                .AddDiscordGateway(p => p.GetRequiredService<IOptions<DiscordBotOptions>>().Value.Token)
                .Configure<DiscordGatewayClientOptions>
                (
                    o =>
                        o.Intents |=
                        GatewayIntents.GuildMessageReactions |
                        GatewayIntents.DirectMessages |
                        GatewayIntents.MessageContents |
                        GatewayIntents.GuildVoiceStates
                )

                // events
                .AddResponder<ChristofelReadyResponder>()
                .AddResponder<ApplicationResponder<IChristofelState, IPluginContext>>()
                .AddScoped(p => new ChristofelReadyResponder(this))

                // commands
                .AddChristofelCommands()
                .AddSingleton<RefreshChristofel>(RefreshAsync);

            serviceCollection.AddCommandTree()
                .WithCommandGroup<ControlCommands>()
                .WithCommandGroup<PluginCommands>();
            return serviceCollection;
        }

        /// <inheritdoc />
        protected override Task InitializeServices
        (
            IServiceProvider services,
            CancellationToken token = default
        )
        {
            _logger = services.GetRequiredService<ILogger<ChristofelApp>>();
            _lifetimeHandler.Logger = _logger;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Initializes the service provider returning context of the plugin.
        /// </summary>
        /// <returns>Context of the plugin.</returns>
        public Task InitAsync() => base.InitAsync();

        /// <summary>
        /// Runs Christofel application in blocking state.
        /// </summary>
        /// <param name="token">The cancellation token for the operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task RunBlockAsync(CancellationToken token = default)
        {
            DiscordBot bot = (DiscordBot)Services.GetRequiredService<IBot>();
            await bot.RunApplication
            (
                CancellationTokenSource
                    .CreateLinkedTokenSource(Lifetime.Stopped, token)
                    .Token
            );
        }

        /// <inheritdoc />
        public override async Task DestroyAsync(CancellationToken token = default)
        {
            foreach (DiscordLoggerProvider provider in Services.GetRequiredService<IEnumerable<ILoggerProvider>>()
                .OfType<DiscordLoggerProvider>())
            {
                provider.Dispose(); // Log all messages
            }

            await base.DestroyAsync(token);
        }

        /// <summary>
        /// Handles <see cref="IReady"/> event.
        /// </summary>
        /// <returns>A result that may not have been successful.</returns>
        public async Task<Result> HandleReady()
        {
            if (!_running)
            {
                try
                {
                    await RunAsync(false, DeferStartable, _lifetimeHandler.Lifetime.Stopped);
                    _logger?.LogInformation("Christofel is ready!");
                }
                catch (Exception e)
                {
                    _logger?.LogCritical(0, e, "Christofel threw an exception when Starting");
                    LifetimeHandler.MoveToError(e);

                    return new InvalidOperationError("Christofel could not start");
                }

                _running = true;
            }

            return Result.FromSuccess();
        }
    }
}