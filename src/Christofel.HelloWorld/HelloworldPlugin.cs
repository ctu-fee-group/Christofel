using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Christofel.BaseLib.Extensions;

namespace Christofel.HelloWorld
{
    public class HelloworldPlugin : DIPlugin
    {
        public override string Name => "Christofel.HelloWorld";
        public override string Description => "Plugin for testing purposes. Supports ping command";
        public override string Version => "v1.0.0";

        protected override IEnumerable<IRefreshable> Refreshable
        {
            get
            {
                yield return Services.GetRequiredService<PingCommandHandler>();
            }
        }

        protected override IEnumerable<IStoppable> Stoppable
        {
            get
            {
                yield return Services.GetRequiredService<PingCommandHandler>();
            }
        }

        protected override IEnumerable<IStartable> Startable
        {
            get
            {
                yield return Services.GetRequiredService<PingCommandHandler>();
            }
        }
        
        protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddDiscordState(State)
                .AddSingleton<PingCommandHandler>()
                .Configure<BotOptions>(State.Configuration.GetSection("Bot"));
        }
        
        protected override Task InitializeServices(IServiceProvider services)
        {
            return Task.CompletedTask;
        }
    }
}