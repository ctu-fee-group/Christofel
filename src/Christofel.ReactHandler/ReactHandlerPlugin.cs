using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.Implementations.Responders;
using Christofel.BaseLib.Lifetime;
using Christofel.BaseLib.Plugins;
using Christofel.CommandsLib;
using Christofel.CommandsLib.Extensions;
using Christofel.ReactHandler.Commands;
using Christofel.ReactHandler.Database;
using Christofel.ReactHandler.Responders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remora.Commands.Extensions;
using Remora.Discord.Gateway.Extensions;

namespace Christofel.ReactHandler
{
    public class ReactHandlerPlugin : DIPlugin
    {
        private PluginLifetimeHandler _lifetimeHandler;
        private ILogger<ReactHandlerPlugin>? _logger;

        public ReactHandlerPlugin()
        {
            _lifetimeHandler = new PluginLifetimeHandler(
                DefaultHandleError(() => _logger),
                DefaultHandleStopRequest(() => _logger));
        }

        public override string Name => "Christofel.ReactHandler";
        public override string Description => "Plugin for handling reacts on messages, marking messages to assign channels/roles on react";
        public override string Version => "v1.0.0";

        protected override IEnumerable<IRefreshable> Refreshable
        {
            get { yield return Services.GetRequiredService<ChristofelCommandRegistrator>(); }
        }

        protected override IEnumerable<IStoppable> Stoppable
        {
            get { yield return Services.GetRequiredService<ChristofelCommandRegistrator>(); }
        }

        protected override IEnumerable<IStartable> Startable
        {
            get { yield return Services.GetRequiredService<ChristofelCommandRegistrator>(); }
        }

        protected override LifetimeHandler LifetimeHandler => _lifetimeHandler;

        protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddDiscordState(State)
                .AddSingleton<PluginResponder>()
                .AddChristofelCommands()
                .AddCommandGroup<HandleReactCommands>()
                .AddResponder<DeleteReactHandlerResponder>()
                .AddResponder<HandleReactResponder>()
                .AddSingleton<ICurrentPluginLifetime>(_lifetimeHandler.LifetimeSpecific)
                .AddDbContextFactory<ReactHandlerContext>()
                .AddTransient<ReactHandlerContext>(p =>
                    p.GetRequiredService<IDbContextFactory<ReactHandlerContext>>().CreateDbContext())
                .AddTransient<ReadOnlyDbContext<ReactHandlerContext>>(p =>
                    p.GetRequiredService<ReadonlyDbContextFactory<ReactHandlerContext>>().CreateDbContext())
                .AddSingleton<ReadonlyDbContextFactory<ReactHandlerContext>>()
                .Configure<BotOptions>(State.Configuration.GetSection("Bot"));
        }

        protected override Task InitializeServices(IServiceProvider services,
            CancellationToken token = new CancellationToken())
        {
            _logger = services.GetRequiredService<ILogger<ReactHandlerPlugin>>();
            Context.PluginResponder = services.GetRequiredService<PluginResponder>();
            return Task.CompletedTask;
        }
    }
}