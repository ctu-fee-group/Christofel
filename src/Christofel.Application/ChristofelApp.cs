using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Application.Assemblies;
using Christofel.Application.Commands;
using Christofel.Application.Logging;
using Christofel.Application.Logging.Discord;
using Christofel.Application.Permissions;
using Christofel.Application.Plugins;
using Christofel.Application.State;
using Christofel.BaseLib;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Discord;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.Lifetime;
using Christofel.BaseLib.Permissions;
using Christofel.BaseLib.Plugins;
using Discord;
using Discord.WebSocket;
using Karambolo.Extensions.Logging.File;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Application
{
    public delegate Task RefreshChristofel(CancellationToken token);
    
    public class ChristofelApp : DIPlugin
    {
        private ILogger<ChristofelApp>? _logger;
        private IConfigurationRoot _configuration;
        private bool _running;
        private ChristofelLifetimeHandler _lifetimeHandler;

        public static IConfigurationRoot CreateConfiguration(string[] commandArgs)
        {
            string environment = Environment.GetEnvironmentVariable("ENV") ?? "production";
            
            return new ConfigurationBuilder()
                .AddJsonFile("config.json", optional: false, reloadOnChange: true)
                .AddJsonFile($@"config.{environment}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(commandArgs)
                .Build();
        }

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
                yield return Services.GetRequiredService<PluginService>();
                yield return Services.GetRequiredService<ControlCommands>();
                yield return Services.GetRequiredService<PluginCommands>();
            }
        }

        protected override IEnumerable<IStoppable> Stoppable
        {
            get
            {
                yield return Services.GetRequiredService<PluginService>();
                yield return Services.GetRequiredService<ControlCommands>();
                yield return Services.GetRequiredService<PluginCommands>();
            }
        }

        protected override IEnumerable<IStartable> Startable
        {
            get
            {
                yield return Services.GetRequiredService<PluginAutoloader>();
                yield return Services.GetRequiredService<ControlCommands>();
                yield return Services.GetRequiredService<PluginCommands>();
            }
        }

        protected override LifetimeHandler LifetimeHandler => _lifetimeHandler;

        protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddSingleton<IChristofelState, ChristofelState>()
                .AddSingleton<IApplicationLifetime>(_lifetimeHandler.LifetimeSpecific)
                // config
                .AddSingleton<IConfiguration>(_configuration)
                .Configure<BotOptions>(_configuration.GetSection("Bot"))
                .Configure<DiscordBotOptions>(_configuration.GetSection("Bot"))
                .Configure<PluginServiceOptions>(_configuration.GetSection("Plugins"))
                .Configure<DiscordSocketConfig>(_configuration.GetSection("Bot:DiscordNet"))
                // bot
                .AddSingleton<DiscordSocketConfig>(
                    s => s.GetRequiredService<IOptions<DiscordSocketConfig>>().Value
                    )
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<IBot, DiscordBot>()
                // db
                .AddDbContextFactory<ChristofelBaseContext>(options => 
                    options
                        .UseMySql(
                            _configuration.GetConnectionString("ChristofelBase"),
                            ServerVersion.AutoDetect(_configuration.GetConnectionString("ChristofelBase")
                            ))
                    )
                .AddSingleton<ReadonlyDbContextFactory<ChristofelBaseContext>>()
                // permissions
                .AddSingleton<IPermissionService, ListPermissionService>()
                .AddTransient<IPermissionsResolver, DbPermissionsResolver>()
                // plugins
                .AddSingleton<PluginService>()
                .AddSingleton<PluginStorage>()
                .AddSingleton<PluginLifetimeService>()
                .AddSingleton<PluginAutoloader>()
                // loggings
                .AddLogging(builder =>
                {
                    builder
                        .AddConfiguration(_configuration.GetSection("Logging"))
                        .ClearProviders()
                        .AddFile()
                        .AddConsole()
                        .AddDiscordLogger();
                })
                .AddSingleton<DiscordNetLog>()
                // commands
                .AddSingleton<ControlCommands>()
                .AddSingleton<PluginCommands>()
                .AddSingleton<RefreshChristofel>(this.RefreshAsync);
        }

        protected override Task InitializeServices(IServiceProvider services, CancellationToken token = new CancellationToken())
        {
            _logger = services.GetRequiredService<ILogger<ChristofelApp>>();
            return Task.CompletedTask;
        }

        public Task InitAsync()
        {
            return base.InitAsync(new CancellationToken());
        }

        public override async Task RunAsync(CancellationToken token = new CancellationToken())
        {
            _logger.LogInformation("Starting Christofel");
            DiscordBot bot = (DiscordBot)Services.GetRequiredService<IBot>();
            
            bot.Client.Ready += HandleReady;

            DiscordNetLog loggerForward = Services.GetRequiredService<DiscordNetLog>();
            loggerForward.RegisterEvents(bot.Client);
            loggerForward.RegisterEvents(bot.Client.Rest);
            
            await bot.StartBotAsync(token);
            await bot.RunApplication(
                CancellationTokenSource
                    .CreateLinkedTokenSource(Lifetime.Stopped, token).Token); 
                // Blocking, ChristofelApp is the only exception
                // that has RunAsync blocking as it's the base entry point.
        }

        public override Task RefreshAsync(CancellationToken token = new CancellationToken())
        {
            _logger.LogInformation("Refreshing Christofel");
            _configuration.Reload();
            return base.RefreshAsync(token);
        }

        public override async Task StopAsync(CancellationToken token = new CancellationToken())
        {
            _logger.LogInformation("Stopping Christofel");
            DiscordBot bot = (DiscordBot)Services.GetRequiredService<IBot>();
            bot.Client.Ready -= HandleReady;

            await base.StopAsync(token);
            await bot.StopBot(token);
        }

        protected Task HandleReady()
        {
            if (!_running)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await base.RunAsync();
                        _logger.LogInformation("Christofel is ready!");
                    }
                    catch (Exception e)
                    {
                        _logger.LogCritical(0, e, "Christofel threw an exception when Starting");
                        LifetimeHandler.MoveToError(e);
                    }
                });
                _running = true;
            }
            
            return Task.CompletedTask;
        }
    }
}