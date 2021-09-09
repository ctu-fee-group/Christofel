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
using Christofel.BaseLib;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Discord;
using Christofel.BaseLib.Implementations.ReadOnlyDatabase;
using Christofel.BaseLib.Permissions;
using Christofel.BaseLib.Plugins;
using Christofel.CommandsLib;
using Christofel.CommandsLib.Extensions;
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
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Extensions;
using Remora.Results;

namespace Christofel.Application
{
    public delegate Task RefreshChristofel(CancellationToken token);

    public class ChristofelApp : ChristofelDIPlugin, IRefreshable, IStoppable
    {
        private readonly IConfigurationRoot _configuration;
        private readonly ChristofelLifetimeHandler _lifetimeHandler;
        private ILogger<ChristofelApp>? _logger;
        private bool _running;

        public ChristofelApp(string[] commandArgs)
        {
            _lifetimeHandler = new ChristofelLifetimeHandler(DefaultHandleError(() => _logger), this);
            _configuration = CreateConfiguration(commandArgs);
        }

        public override string Name => "Christofel.Application";
        public override string Description => "Base application with module commands managing their lifecycle";
        public override string Version => "v0.0.1";

        protected override IEnumerable<IRefreshable> Refreshable
        {
            get
            {
                yield return this;
                yield return Services.GetRequiredService<PluginAutoloader>();
                yield return Services.GetRequiredService<ChristofelCommandRegistrator>();
            }
        }

        protected override IEnumerable<IStoppable> Stoppable
        {
            get
            {
                yield return this;
                yield return Services.GetRequiredService<PluginAutoloader>();
                yield return Services.GetRequiredService<ChristofelCommandRegistrator>();
            }
        }

        protected override IEnumerable<IStartable> Startable => Enumerable.Empty<IStartable>();

        /// <summary>
        ///     Start after Ready event was received
        /// </summary>
        private IEnumerable<IStartable> DeferStartable
        {
            get
            {
                yield return Services.GetRequiredService<PluginAutoloader>();
                yield return Services.GetRequiredService<ChristofelCommandRegistrator>();
            }
        }

        protected override LifetimeHandler LifetimeHandler => _lifetimeHandler;

        Task IRefreshable.RefreshAsync(CancellationToken token)
        {
            _logger.LogInformation("Refreshing Christofel");
            _configuration.Reload();
            return Task.CompletedTask;
        }

        Task IStoppable.StopAsync(CancellationToken token)
        {
            _logger.LogInformation("Stopping Christofel");
            return Task.CompletedTask;
        }

        public static IConfigurationRoot CreateConfiguration(string[] commandArgs)
        {
            string environment = Environment.GetEnvironmentVariable("ENV") ?? "production";

            return new ConfigurationBuilder()
                .AddJsonFile("config.json", false, true)
                .AddJsonFile($@"config.{environment}.json", true, true)
                .AddEnvironmentVariables()
                .AddCommandLine(commandArgs)
                .Build();
        }

        protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddSingleton<IChristofelState, ChristofelState>()
                .AddSingleton(_lifetimeHandler.LifetimeSpecific)
                // plugins
                .AddPlugins()
                .AddTransient<PluginAutoloader>()
                .AddRuntimePlugins<IChristofelState, IPluginContext>()
                // config
                .AddSingleton<IConfiguration>(_configuration)
                .Configure<BotOptions>(_configuration.GetSection("Bot"))
                .Configure<DiscordBotOptions>(_configuration.GetSection("Bot"))
                .Configure<PluginServiceOptions>(_configuration.GetSection("Plugins"))
                .Configure<PluginAutoloaderOptions>(_configuration.GetSection("Plugins"))
                .AddSingleton<IBot, DiscordBot>()
                // db
                .AddDbContextFactory<ChristofelBaseContext>
                (
                    options =>
                        options
                            .UseMySql
                            (
                                _configuration.GetConnectionString("ChristofelBase"),
                                ServerVersion.AutoDetect(_configuration.GetConnectionString("ChristofelBase"))
                            )
                )
                .AddTransient
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
                        o.Intents |= GatewayIntents.GuildMessageReactions | GatewayIntents.DirectMessages
                )
                // events
                .AddResponder<ChristofelReadyResponder>()
                .AddResponder<ApplicationResponder<IChristofelState, IPluginContext>>()
                .AddScoped(p => new ChristofelReadyResponder(this))
                // commands
                .AddChristofelCommands()
                .AddCommandGroup<ControlCommands>()
                .AddCommandGroup<PluginCommands>()
                .AddSingleton<RefreshChristofel>(RefreshAsync);
        }

        protected override Task InitializeServices
        (
            IServiceProvider services,
            CancellationToken token = new CancellationToken()
        )
        {
            _logger = services.GetRequiredService<ILogger<ChristofelApp>>();
            _lifetimeHandler.Logger = _logger;
            return Task.CompletedTask;
        }

        public Task InitAsync() => base.InitAsync();

        public async Task RunBlockAsync(CancellationToken token = default)
        {
            DiscordBot bot = (DiscordBot) Services.GetRequiredService<IBot>();
            await bot.RunApplication
            (
                CancellationTokenSource
                    .CreateLinkedTokenSource(Lifetime.Stopped, token).Token
            );
        }

        public override async Task DestroyAsync(CancellationToken token = new CancellationToken())
        {
            foreach (DiscordLoggerProvider provider in Services.GetRequiredService<IEnumerable<ILoggerProvider>>()
                .OfType<DiscordLoggerProvider>())
            {
                provider.Dispose(); // Log all messages
            }

            await base.DestroyAsync(token);
        }

        public async Task<Result> HandleReady()
        {
            if (!_running)
            {
                try
                {
                    await base.RunAsync(false, DeferStartable, _lifetimeHandler.Lifetime.Stopped);
                    _logger.LogInformation("Christofel is ready!");
                }
                catch (Exception e)
                {
                    _logger.LogCritical(0, e, "Christofel threw an exception when Starting");
                    LifetimeHandler.MoveToError(e);

                    return new InvalidOperationError("Christofel could not start");
                }

                _running = true;
            }

            return Result.FromSuccess();
        }
    }
}