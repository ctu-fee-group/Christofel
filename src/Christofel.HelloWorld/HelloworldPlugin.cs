using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.Lifetime;
using Microsoft.Extensions.Logging;

namespace Christofel.HelloWorld
{
    public class HelloworldPlugin : DIPlugin
    {
        private PluginLifetimeHandler _lifetimeHandler;
        private ILogger<HelloworldPlugin>? _logger;

        public HelloworldPlugin()
        {
            _lifetimeHandler = new PluginLifetimeHandler(
                DefaultHandleError(() => _logger),
                DefaultHandleStopRequest(() => _logger));
        }
        
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

        protected override LifetimeHandler LifetimeHandler => _lifetimeHandler;

        protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddDiscordState(State)
                .AddSingleton<ICurrentPluginLifetime>(_lifetimeHandler.LifetimeSpecific)
                .AddSingleton<PingCommandHandler>()
                .Configure<BotOptions>(State.Configuration.GetSection("Bot"));
        }
        
        protected override Task InitializeServices(IServiceProvider services, CancellationToken token = new CancellationToken())
        {
            _logger = services.GetRequiredService<ILogger<HelloworldPlugin>>();
            token.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }
}