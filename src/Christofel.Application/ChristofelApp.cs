using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Christofel.Application.Commands;
using Christofel.Application.Configuration;
using Christofel.Application.Logging.Discord;
using Christofel.Application.Permissions;
using Christofel.Application.Plugins;
using Christofel.Application.State;
using Christofel.BaseLib;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Configuration.Converters;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Discord;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.Permissions;
using Christofel.BaseLib.Plugins;
using Discord;
using Karambolo.Extensions.Logging.File;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Christofel.Application
{
    public class ChristofelApp : DIPlugin
    {
        public static string JsonConfigPath => "config.json";

        private IReadableConfig _jsonConfig;
        private IConfigConverterResolver _converterResolver;
        private ILogger<ChristofelApp>? _logger;

        public ChristofelApp()
        {
            _converterResolver = new ThreadSafeConverterResolver();
            _converterResolver.AddConvertConverters();
            _converterResolver.AddEnumConverter<LogLevel>();
            _converterResolver.AddIEnumerableConverter<string>();
            
            _jsonConfig = new JsonConfig(JsonConfigPath, _converterResolver);
            _jsonConfig.ExternalResolver.AddConvertConverters();
        }

        public override string Name => "Application";
        public override string Description => "Base application with module commands managing their lifecycle";
        public override string Version => "v0.0.1";

        protected override IEnumerable<IRefreshable> Refreshable
        {
            get
            {
                // TODO: modules state
                // TODO: logger
                
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
                // bot
                .AddSingleton<IBot, DiscordBot>()
                // db
                .AddDbContextFactory<ChristofelBaseContext>(async options => options.
                    UseMySql(await _jsonConfig.GetAsync<string>("db.connectionstring"), ServerVersion.AutoDetect(await _jsonConfig.GetAsync<string>("db.connectionstring"))))
                .AddSingleton<ReadonlyDbContextFactory<ChristofelBaseContext>>()
                // permissions
                .AddSingleton<IPermissionService, ListPermissionService>()
                .AddTransient<IPermissionsResolver, DbPermissionsResolver>()
                // plugins
                .AddSingleton<PluginService>() // TODO: add options
                // config
                .AddSingleton<IConfigConverterResolver>(_converterResolver)
                .AddSingleton<DbConfig>()
                .AddSingleton(_jsonConfig)
                .AddSingleton<IWRConfig, CompositeConfig>(services =>
                {
                    return new CompositeConfig(
                        new IReadableConfig[]
                        {
                            services.GetRequiredService<JsonConfig>(),
                            services.GetRequiredService<DbConfig>()
                        },
                        services.GetRequiredService<DbConfig>(),
                        services.GetRequiredService<IConfigConverterResolver>()
                        );
                })
                .AddSingleton<IReadableConfig>(s => s.GetRequiredService<IWRConfig>())
                .AddSingleton<IWritableConfig>(s => s.GetRequiredService<IWRConfig>())
                // logging
                .AddSingleton(typeof(ILogger<>), typeof(Logger<>))
                .AddSingleton<ILoggerFactory>(services =>
                {
                    IReadableConfig appConfig = services.GetRequiredService<IReadableConfig>();

                    return LoggerFactory.Create(config =>
                    {
                        // TODO: use Json config instead
                        config
                            .ClearProviders()
                            .AddConsole()
                            .AddFile(config =>
                            {
                                config.BasePath = "logs";
                                
                                config.Files = new[]
                                {
                                    new LogFileOptions
                                    {
                                        Path = "default-<counter>.log",
                                        MaxFileSize = 10 * 1024 * 1024 // 10 MB
                                    }
                                };
                            })
                            .AddDiscordLogger(config =>
                            {
                                config.ChannelId = appConfig.GetAsync<ulong>("discord.logging.warning").GetAwaiter().GetResult();
                                config.MinLevel = LogLevel.Warning;
                            })
                            .AddDiscordLogger(config =>
                            {
                                config.ChannelId = appConfig.GetAsync<ulong>("discord.logging.info").GetAwaiter().GetResult();
                                config.MinLevel = LogLevel.Trace; // TODO: change depending on debug, in config maybe?
                            });
                    });
                })
                // commands
                .AddSingleton<ControlCommands>()
                .AddSingleton<PluginCommands>()
                .AddSingleton(this);
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
            DiscordBot bot = (DiscordBot)Services.GetRequiredService<IBot>();
            bot.Client.Ready += HandleReady;

            await bot.StartBotAsync();
            await bot.RunApplication(); 
                // Blocking, ChristofelApp is the only exception
                // that has RunAsync blocking as it's the base entry point.
        }

        public override async Task StopAsync()
        {
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
            
            _logger?.LogInformation("Christofel is ready!");
            return  base.RunAsync();
        }
    }
}