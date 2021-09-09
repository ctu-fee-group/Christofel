using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Christofel.BaseLib.Extensions;
using Christofel.CommandsLib;
using Christofel.CommandsLib.Extensions;
using Christofel.Plugins.Lifetime;
using Christofel.Plugins.Runtime;
using Christofel.Remora.Responders;
using Microsoft.Extensions.Logging;
using Remora.Commands.Extensions;

namespace Christofel.HelloWorld
{
    public class HelloworldPlugin : ChristofelDIPlugin
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
                yield return Services.GetRequiredService<ChristofelCommandRegistrator>();
            }
        }

        protected override IEnumerable<IStoppable> Stoppable
        {
            get
            {
                yield return Services.GetRequiredService<ChristofelCommandRegistrator>();

            }
        }

        protected override IEnumerable<IStartable> Startable
        {
            get
            {
                yield return Services.GetRequiredService<ChristofelCommandRegistrator>();
            }
        }

        protected override LifetimeHandler LifetimeHandler => _lifetimeHandler;

        protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddDiscordState(State)
                .AddSingleton<PluginResponder>()
                .AddChristofelCommands()
                .AddCommandGroup<PingCommandGroup>()
                .AddSingleton<ICurrentPluginLifetime>(_lifetimeHandler.LifetimeSpecific)
                .Configure<BotOptions>(State.Configuration.GetSection("Bot"));
        }
        
        protected override Task InitializeServices(IServiceProvider services, CancellationToken token = new CancellationToken())
        {
            _logger = services.GetRequiredService<ILogger<HelloworldPlugin>>();
            ((PluginContext)Context).PluginResponder = services.GetRequiredService<PluginResponder>();
            return Task.CompletedTask;
        }
    }
}