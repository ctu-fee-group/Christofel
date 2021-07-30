using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.ComTypes;
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
using Christofel.BaseLib.Extensions;
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
    public delegate Task RefreshChristofel();
    
    public class ChristofelApp : DIPlugin
    {
        private ILogger<ChristofelApp>? _logger;
        private IConfigurationRoot _configuration;

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

        protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddSingleton<IChristofelState, ChristofelState>()
                // config
                .AddSingleton<IConfiguration>(_configuration)
                .Configure<BotOptions>(_configuration.GetSection("Bot"))
                .Configure<DiscordBotOptions>(_configuration.GetSection("Bot"))
                .Configure<PluginServiceOptions>(_configuration.GetSection("Plugins"))
                .Configure<DiscordSocketConfig>(_configuration.GetSection("Bot:DiscordNet"))
                // bot
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

        protected override Task InitializeServices(IServiceProvider services)
        {
            _logger = services.GetRequiredService<ILogger<ChristofelApp>>();
            return Task.CompletedTask;
        }

        public new Task InitAsync()
        {
            return base.InitAsync();
        }

        public override async Task RunAsync()
        {
            _logger.LogInformation("Starting Christofel");
            DiscordBot bot = (DiscordBot)Services.GetRequiredService<IBot>();
            
            bot.Client.Ready += HandleReady;

            DiscordNetLog loggerForward = Services.GetRequiredService<DiscordNetLog>();
            loggerForward.RegisterEvents(bot.Client);
            loggerForward.RegisterEvents(bot.Client.Rest);
            
            await bot.StartBotAsync();
            await bot.RunApplication(); 
                // Blocking, ChristofelApp is the only exception
                // that has RunAsync blocking as it's the base entry point.
        }

        public override Task RefreshAsync()
        {
            _logger.LogInformation("Refreshing Christofel");
            _configuration.Reload();
            return base.RefreshAsync();
        }

        public override async Task StopAsync()
        {
            _logger.LogInformation("Stopping Christofel");
            DiscordBot bot = (DiscordBot)Services.GetRequiredService<IBot>();
            bot.Client.Ready -= HandleReady;

            await bot.StopBot();
            await base.StopAsync();
        }

        protected Task HandleReady()
        {
            _logger.LogInformation("Christofel is ready!");
            return base.RunAsync();
        }
    }
}