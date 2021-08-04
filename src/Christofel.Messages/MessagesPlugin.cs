using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.Lifetime;
using Christofel.BaseLib.Plugins;
using Christofel.CommandsLib.Commands;
using Christofel.CommandsLib.Extensions;
using Christofel.CommandsLib.Handlers;
using Christofel.Messages.Commands;
using Christofel.Messages.Options;
using Christofel.Messages.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Christofel.Messages
{
    public class MessagesPlugin : DIPlugin
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
                yield return Services.GetRequiredService<CommandsRegistrator>();
            }
        }
        
        protected override IEnumerable<IStoppable> Stoppable
        {
            get
            {
                yield return Services.GetRequiredService<InteractionHandler>();
                yield return Services.GetRequiredService<CommandsRegistrator>();
            }
        }
        
        protected override IEnumerable<IStartable> Startable
        {
            get
            {
                yield return Services.GetRequiredService<InteractionHandler>();
                yield return Services.GetRequiredService<CommandsRegistrator>();
            }
        }

        protected override LifetimeHandler LifetimeHandler => _lifetimeHandler;
        protected override Task InitializeServices(IServiceProvider services, CancellationToken token = new CancellationToken())
        {
            _logger = services.GetRequiredService<ILogger<MessagesPlugin>>();
            return Task.CompletedTask;
        }

        protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddDiscordState(State)
                .AddSingleton<ICurrentPluginLifetime>(_lifetimeHandler.LifetimeSpecific)
                .AddSingleton<EmbedsProvider>()
                .Configure<EmbedsOptions>(State.Configuration.GetSection("Messages:Embeds"))
                .Configure<BotOptions>(State.Configuration.GetSection("Bot"))
                .AddDefaultInteractionHandler(collection =>
                    collection
                        .AddCommandGroup<EchoCommandGroup>()
                        .AddCommandGroup<EmbedCommandGroup>()
                        .AddCommandGroup<ReactCommandGroup>()
                    );
        }
    }
}