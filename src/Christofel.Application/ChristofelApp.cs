using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Christofel.Application.Commands;
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
        public static string JsonConfigPath => "config.json";

        private ILogger<ChristofelApp>? _logger;
        private IConfiguration _configuration;

        public ChristofelApp(string[] args)
        {
            string environment = Environment.GetEnvironmentVariable("ENV") ?? "production";
            
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("config.json")
                .AddJsonFile($@"config.{environment}.json")
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();
        }

        public override string Name => "Application";
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
                // bot
                .AddSingleton<IBot, DiscordBot>()
                // db
                .AddDbContextFactory<ChristofelBaseContext>(async options => 
                    options
                        .UseMySql(_configuration["Db:ConnectionString"], ServerVersion.AutoDetect(_configuration["Db:ConnectionString"]))
                    )
                .AddSingleton<ReadonlyDbContextFactory<ChristofelBaseContext>>()
                // permissions
                .AddSingleton<IPermissionService, ListPermissionService>()
                .AddTransient<IPermissionsResolver, DbPermissionsResolver>()
                // plugins
                .AddSingleton<PluginService>()
                // logging
                .AddLogging(builder =>
                {
                    builder
                        .AddConfiguration(_configuration.GetSection("Logging"))
                        .ClearProviders()
                        .AddFile()
                        .AddConsole()
                        .AddDiscordLogger();
                })
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

            await bot.StartBotAsync();
            await bot.RunApplication(); 
                // Blocking, ChristofelApp is the only exception
                // that has RunAsync blocking as it's the base entry point.
        }

        public override Task RefreshAsync()
        {
            _logger.LogInformation("Refreshing Christofel");
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
            // TODO: register commands
            // TODO: register initial modules that were in previous run of the application
            //   - if one module fails, just skip it
            
            _logger.LogInformation("Christofel is ready!");
            return  base.RunAsync();
        }
    }
}