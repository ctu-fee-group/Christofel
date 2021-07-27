using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Christofel.BaseLib;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Discord;
using Christofel.BaseLib.Plugins;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.Application
{
    public class ChristofelApp : DIPlugin
    {
        public static string JsonConfigPath => "config.json";
        
        public override string Name => "Application";
        public override string Description => "Base application with module commands managing their lifecycle";
        public override string Version => "v0.0.1";

        private IReadableConfig _jsonConfig;
        
        protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddDbContextFactory<ChristofelBaseContext>(async options => options.
                    UseMySql(await _jsonConfig.GetAsync<string>("db.connectionstring"), ServerVersion.AutoDetect(await _jsonConfig.GetAsync<string>("db.connectionstring"))))
                .AddSingleton<IBot, DiscordBot>()
                .AddSingleton<IChristofelState, ChristofelState>()
                .AddSingleton<DbConfig>()
                .AddSingleton(_jsonConfig)
                .AddSingleton<IConfig, CompositeConfig>(services =>
                {
                    return new CompositeConfig(
                        new IReadableConfig[]{services.GetRequiredService<JsonConfig>(), services.GetRequiredService<DbConfig>()},
                        services.GetRequiredService<DbConfig>());
                });
        }

        protected override Task InitializeServices(IServiceProvider services)
        {
            IBot bot = services.GetRequiredService<IBot>();

            bot.Client.Ready += HandleReady;

            return Task.CompletedTask;
        }

        public new Task InitAsync()
        {
            _jsonConfig = new JsonConfig(JsonConfigPath);
            return base.InitAsync();
        }

        public override async Task DestroyAsync()
        {
            await base.DestroyAsync();
            
            if (Services == null)
            {
                throw new InvalidOperationException("Services are supposed to be initialized before destroying the application");
            }
            
            DiscordBot bot = (DiscordBot)Services.GetRequiredService<IBot>();
            await bot.StopBot();
        }

        public async Task RunAsync()
        {
            if (Services == null)
            {
                throw new InvalidOperationException("Services are supposed to be initialized before running the application");
            }
            
            DiscordBot bot = (DiscordBot)Services.GetRequiredService<IBot>();

            await bot.StartBotAsync();
            await bot.RunApplication();
        }

        protected Task HandleReady()
        {
            return Task.CompletedTask;
        }
    }
}