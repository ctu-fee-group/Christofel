//
//   ManagementPlugin.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.Implementations.ReadOnlyDatabase;
using Christofel.BaseLib.Implementations.Storages;
using Christofel.BaseLib.Plugins;
using Christofel.CommandsLib;
using Christofel.CommandsLib.Extensions;
using Christofel.Management.Commands;
using Christofel.Management.CtuUtils;
using Christofel.Management.Database;
using Christofel.Management.Slowmode;
using Christofel.Plugins.Lifetime;
using Christofel.Plugins.Runtime;
using Christofel.Remora.Responders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remora.Commands.Extensions;

namespace Christofel.Management
{
    public class ManagementPlugin : ChristofelDIPlugin
    {
        private readonly PluginLifetimeHandler _lifetimeHandler;
        private ILogger<ManagementPlugin>? _logger;

        public ManagementPlugin()
        {
            _lifetimeHandler = new PluginLifetimeHandler
            (
                DefaultHandleError(() => _logger),
                DefaultHandleStopRequest(() => _logger)
            );
        }

        public override string Name => "Christofel.Management";

        public override string Description =>
            "Plugin for user and messages management. Supports basic management commands.";

        public override string Version => "v1.0.0";

        protected override IEnumerable<IRefreshable> Refreshable
        {
            get { yield return Services.GetRequiredService<ChristofelCommandRegistrator>(); }
        }

        protected override IEnumerable<IStoppable> Stoppable
        {
            get
            {
                yield return Services.GetRequiredService<ChristofelCommandRegistrator>();
                yield return Services.GetRequiredService<SlowmodeAutorestore>();
            }
        }

        protected override IEnumerable<IStartable> Startable
        {
            get
            {
                yield return Services.GetRequiredService<ChristofelCommandRegistrator>();
                yield return Services.GetRequiredService<SlowmodeAutorestore>();
            }
        }

        protected override LifetimeHandler LifetimeHandler => _lifetimeHandler;

        protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection)
        {
            return serviceCollection
                // Christofel
                .AddDiscordState(State)
                // Databases
                .AddChristofelDatabase(State)
                .AddDbContextFactory<ManagementContext>
                (
                    options => options
                        .UseMySql
                        (
                            State.Configuration.GetConnectionString("Management"),
                            ServerVersion.AutoDetect(State.Configuration.GetConnectionString("Management"))
                        )
                )
                .AddTransient
                (
                    p =>
                        p.GetRequiredService<IDbContextFactory<ManagementContext>>().CreateDbContext()
                )
                .AddReadOnlyDbContext<ManagementContext>()
                // Service for resolving ctu identities
                .AddSingleton<CtuIdentityResolver>()
                // Responder for every event to delegate to other registered responders
                .AddSingleton<PluginResponder>()
                // Commands
                .AddChristofelCommands()
                .AddCommandGroup<MessageCommandsGroup>()
                .AddCommandGroup<PermissionCommandsGroup>()
                .AddCommandGroup<UserCommandsGroup>()
                // Slowmodes
                .AddSingleton<IThreadSafeStorage<RegisteredTemporalSlowmode>,
                    ThreadSafeListStorage<RegisteredTemporalSlowmode>>()
                .AddTransient<SlowmodeService>()
                .AddTransient<SlowmodeAutorestore>()
                // Misc
                .AddSingleton(_lifetimeHandler.LifetimeSpecific)
                // Configurations
                .Configure<BotOptions>(State.Configuration.GetSection("Bot"));
        }

        protected override Task InitializeServices
        (
            IServiceProvider services,
            CancellationToken token = new CancellationToken()
        )
        {
            _logger = services.GetRequiredService<ILogger<ManagementPlugin>>();
            ((PluginContext) Context).PluginResponder = services.GetRequiredService<PluginResponder>();
            return Task.CompletedTask;
        }
    }
}