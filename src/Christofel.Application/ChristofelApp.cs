using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
using Christofel.BaseLib.Lifetime;
using Christofel.BaseLib.Permissions;
using Christofel.BaseLib.Plugins;
using Christofel.CommandsLib;
using Discord.Net.Interactions.DI;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Application
{
    public delegate Task RefreshChristofel(CancellationToken token);
    
    public class ChristofelApp : DIPlugin, IStartable, IRefreshable, IStoppable
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
                yield return this;
                yield return Services.GetRequiredService<PluginService>();
                yield return Services.GetRequiredService<InteractionsService>();
            }
        }

        protected override IEnumerable<IStoppable> Stoppable
        {
            get
            {
                yield return this;
                yield return Services.GetRequiredService<PluginService>();
                yield return Services.GetRequiredService<InteractionsService>();

            }
        }

        protected override IEnumerable<IStartable> Startable
        {
            get
            {
                yield return this;
            }
        }

        /// <summary>
        /// Start after Ready event was received
        /// </summary>
        private IEnumerable<IStartable> DeferStartable
        {
            get
            {
                yield return Services.GetRequiredService<PluginAutoloader>();
                yield return Services.GetRequiredService<InteractionsService>();
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
                .AddSingleton<DiscordRestClient>(p => p.GetRequiredService<DiscordSocketClient>().Rest)
                .AddSingleton<IBot, DiscordBot>()
                // db
                .AddPooledDbContextFactory<ChristofelBaseContext>(options => 
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
                        .AddSimpleConsole(options => options.IncludeScopes = true)
                        .AddDiscordLogger();
                })
                .AddSingleton<DiscordNetLog>()
                // commands
                .AddChristofelInteractionService()
                .AddCommandGroup<ControlCommands>()
                .AddCommandGroup<PluginCommands>()
                .AddSingleton<RefreshChristofel>(this.RefreshAsync);
        }

        protected override Task InitializeServices(IServiceProvider services, CancellationToken token = new CancellationToken())
        {
            _logger = services.GetRequiredService<ILogger<ChristofelApp>>();
            _lifetimeHandler.Logger = _logger;
            return Task.CompletedTask;
        }

        public Task InitAsync()
        {
            return base.InitAsync(new CancellationToken());
        }

        public Task RunBlockAsync(CancellationToken token = default)
        {
            DiscordBot bot = (DiscordBot)Services.GetRequiredService<IBot>();
            return bot.RunApplication(
                CancellationTokenSource
                    .CreateLinkedTokenSource(Lifetime.Stopped, token).Token);
        }

        public override async Task DestroyAsync(CancellationToken token = new CancellationToken())
        {
            foreach (DiscordLoggerProvider provider in Services.GetRequiredService<IEnumerable<ILoggerProvider>>()
                .OfType<DiscordLoggerProvider>())
            {
                provider.Dispose(); // Log all messages
            }
            
            DiscordBot bot = (DiscordBot)Services.GetRequiredService<IBot>();
            await bot.StopBot(token); // Stop bot before disposing
            await base.DestroyAsync(token);
        }

        protected Task HandleReady()
        {
            if (!_running)
            {
                Task.Run(async () =>
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
                    }
                });
                _running = true;
            }
            
            return Task.CompletedTask;
        }
        
        Task IStartable.StartAsync(CancellationToken token)
        {
            _logger.LogInformation("Starting Christofel");
            DiscordBot bot = (DiscordBot)Services.GetRequiredService<IBot>();
            
            bot.Client.Ready += HandleReady;

            DiscordNetLog loggerForward = Services.GetRequiredService<DiscordNetLog>();
            loggerForward.RegisterEvents(bot.Client);
            loggerForward.RegisterEvents(bot.Client.Rest);

            return bot.StartBotAsync(token);
        }
        
        Task IRefreshable.RefreshAsync(CancellationToken token)
        {
            _logger.LogInformation("Refreshing Christofel");
            _configuration.Reload();
            return Task.CompletedTask;
        }
        
        Task IStoppable.StopAsync(CancellationToken token)
        {
            _logger.LogInformation("Stopping Christofel");
            DiscordBot bot = (DiscordBot)Services.GetRequiredService<IBot>();
            bot.Client.Ready -= HandleReady;
            return Task.CompletedTask;
        }
    }
}