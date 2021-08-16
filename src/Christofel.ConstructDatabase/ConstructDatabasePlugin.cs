using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.Lifetime;
using Christofel.BaseLib.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Christofel.ConstructDatabase
{
    public class ConstructDatabasePlugin : DIPlugin
    {
        private ILogger _logger;

        public ConstructDatabasePlugin()
        {
            LifetimeHandler = new PluginLifetimeHandler(DefaultHandleError(() => _logger),
                DefaultHandleStopRequest(() => _logger));
        }

        public override string Name => "Christofel.ConstructDatabase";
        public override string Description => "Plugin to construct database data";
        public override string Version { get; }
        protected override IEnumerable<IRefreshable> Refreshable => Enumerable.Empty<IRefreshable>();
        protected override IEnumerable<IStoppable> Stoppable => Enumerable.Empty<IStoppable>();

        protected override IEnumerable<IStartable> Startable
        {
            get
            {
                yield return Services.GetRequiredService<DatabaseBuilder>();
            }
        }
        
        protected override LifetimeHandler LifetimeHandler { get; }

        protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddDiscordState(State)
                .AddChristofelDatabase(State)
                .AddSingleton<ICurrentPluginLifetime>((ICurrentPluginLifetime)LifetimeHandler.Lifetime)
                .AddSingleton<DatabaseBuilder>()
                .Configure<BotOptions>(State.Configuration.GetSection("Bot"));
        }

        protected override Task InitializeServices(IServiceProvider services,
            CancellationToken token = new CancellationToken())
        {
            _logger = Services.GetRequiredService<ILogger<ConstructDatabasePlugin>>();
            return Task.CompletedTask;
        }
    }
}