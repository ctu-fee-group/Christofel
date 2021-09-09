using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.Plugins;
using Christofel.CommandsLib;
using Christofel.CommandsLib.Extensions;
using Christofel.Messages.Commands;
using Christofel.Messages.Options;
using Christofel.Messages.Services;
using Christofel.Plugins.Lifetime;
using Christofel.Plugins.Runtime;
using Christofel.Remora.Responders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remora.Commands.Extensions;

namespace Christofel.Messages
{
    public class MessagesPlugin : ChristofelDIPlugin
    {
        private PluginLifetimeHandler _lifetimeHandler;
        private ILogger<MessagesPlugin>? _logger;
        
        public MessagesPlugin()
        {
            _lifetimeHandler = new PluginLifetimeHandler(DefaultHandleError(() => _logger),
                DefaultHandleStopRequest(() => _logger));
        }
        
        public override string Name => "Christofel.Messages";
        public override string Description => "Plugin for handling commands with messages";
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
        protected override Task InitializeServices(IServiceProvider services, CancellationToken token = new CancellationToken())
        {
            _logger = services.GetRequiredService<ILogger<MessagesPlugin>>();
            ((PluginContext)Context).PluginResponder = services.GetRequiredService<PluginResponder>();
            return Task.CompletedTask;
        }

        protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddDiscordState(State)
                .AddSingleton<ICurrentPluginLifetime>(_lifetimeHandler.LifetimeSpecific)
                .AddSingleton<EmbedsProvider>()
                .AddSingleton<PluginResponder>()
                .Configure<EmbedsOptions>(State.Configuration.GetSection("Messages:Embeds"))
                .Configure<BotOptions>(State.Configuration.GetSection("Bot"))
                .AddChristofelCommands()
                .AddCommandGroup<EchoCommandGroup>()
                .AddCommandGroup<EmbedCommandGroup>()
                .AddCommandGroup<ReactCommandGroup>();
        }
    }
}