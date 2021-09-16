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
using Christofel.BaseLib.Plugins;
using Christofel.CommandsLib;
using Christofel.CommandsLib.Extensions;
using Christofel.Helpers.ReadOnlyDatabase;
using Christofel.Helpers.Storages;
using Christofel.Management.Commands;
using Christofel.Management.CtuUtils;
using Christofel.Management.Database;
using Christofel.Management.Jobs;
using Christofel.Management.ResendRule;
using Christofel.Management.Slowmode;
using Christofel.Plugins;
using Christofel.Plugins.Lifetime;
using Christofel.Plugins.Runtime;
using Christofel.Remora.Responders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remora.Commands.Extensions;
using Remora.Discord.Gateway.Extensions;

namespace Christofel.Management
{
    /// <summary>
    /// Plugin for admins and moderators to manage users, messages, permissions etc.
    /// </summary>
    public class ManagementPlugin : ChristofelDIPlugin
    {
        private readonly PluginLifetimeHandler _lifetimeHandler;
        private ILogger<ManagementPlugin>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagementPlugin"/> class.
        /// </summary>
        public ManagementPlugin()
        {
            _lifetimeHandler = new PluginLifetimeHandler
            (
                DefaultHandleError(() => _logger),
                DefaultHandleStopRequest(() => _logger)
            );
        }

        /// <inheritdoc />
        public override string Name => "Christofel.Management";

        /// <inheritdoc />
        public override string Description =>
            "Plugin for user and messages management. Supports basic management commands.";

        /// <inheritdoc />
        public override string Version => "v1.0.0";

        /// <inheritdoc />
        protected override LifetimeHandler LifetimeHandler => _lifetimeHandler;

        /// <inheritdoc />
        protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection)
        {
            return serviceCollection

                // Christofel
                .AddDiscordState(State)

                // Scheduler
                .AddPluginScheduler()
                .AddSchedulerJob<SlowmodeDisableJob>()
                .AddSchedulerJob<RemoveOldUsersJob>()
                .AddSingleton<CronJobs>()

                // Databases
                .AddChristofelDatabase(State)
                .AddChristofelDbContextFactory<ManagementContext>(State.Configuration)
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
                .AddCommandGroup<ResendRuleCommandsGroup>()

                // Responders
                .AddResponder<ResendRuleResponder>()
                .AddMemoryCache()
                .AddHttpClient()

                // Slowmodes
                .AddSingleton<IThreadSafeStorage<RegisteredTemporalSlowmode>,
                    ThreadSafeListStorage<RegisteredTemporalSlowmode>>()
                .AddTransient<SlowmodeService>()
                .AddStateful<SlowmodeAutorestore>(ServiceLifetime.Transient)

                // Misc
                .AddSingleton(_lifetimeHandler.LifetimeSpecific)

                // Configurations
                .Configure<ResendRuleOptions>(State.Configuration.GetSection("Management:Resend"))
                .Configure<BotOptions>(State.Configuration.GetSection("Bot"));
        }

        /// <inheritdoc />
        protected override Task InitializeServices
        (
            IServiceProvider services,
            CancellationToken token = default
        )
        {
            _logger = services.GetRequiredService<ILogger<ManagementPlugin>>();
            ((PluginContext)Context).PluginResponder = services.GetRequiredService<PluginResponder>();
            return Task.CompletedTask;
        }
    }
}
