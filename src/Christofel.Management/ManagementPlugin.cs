using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.Lifetime;
using Christofel.BaseLib.Plugins;
using Christofel.CommandsLib;
using Christofel.Management.Commands;
using Christofel.Management.CtuUtils;
using Discord.Net.Interactions.DI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Christofel.Management
{
    public class ManagementPlugin : DIPlugin
    {
        private PluginLifetimeHandler _lifetimeHandler;
        private ILogger<ManagementPlugin>? _logger;

        public ManagementPlugin()
        {
            _lifetimeHandler = new PluginLifetimeHandler(
                DefaultHandleError(() => _logger),
                DefaultHandleStopRequest(() => _logger));
        }

        public override string Name => "Christofel.Management";

        public override string Description =>
            "Plugin for user and messages management. Supports basic management commands.";

        public override string Version => "v1.0.0";

        protected override IEnumerable<IRefreshable> Refreshable
        {
            get
            {
                yield return Services.GetRequiredService<InteractionsService>();
            }
        }

        protected override IEnumerable<IStoppable> Stoppable
        {
            get
            {
                yield return Services.GetRequiredService<InteractionsService>();

            }
        }

        protected override IEnumerable<IStartable> Startable
        {
            get
            {
                yield return Services.GetRequiredService<InteractionsService>();
            }
        }

        protected override LifetimeHandler LifetimeHandler => _lifetimeHandler;

        protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddDiscordState(State)
                .AddChristofelDatabase(State)
                .AddSingleton<CtuIdentityResolver>()
                .AddCommandGroup<MessageCommandsGroup, PermissionSlashInfo>()
                .AddCommandGroup<PermissionCommandsGroup, PermissionSlashInfo>()
                .AddCommandGroup<UserCommandsGroup, PermissionSlashInfo>()
                .AddSingleton<ICurrentPluginLifetime>(_lifetimeHandler.LifetimeSpecific)
                .Configure<BotOptions>(State.Configuration.GetSection("Bot"));
        }

        protected override Task InitializeServices(IServiceProvider services,
            CancellationToken token = new CancellationToken())
        {
            _logger = services.GetRequiredService<ILogger<ManagementPlugin>>();
            token.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }
}