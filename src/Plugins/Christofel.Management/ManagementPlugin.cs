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
using Christofel.CtuAuth;
using Christofel.CtuAuth.Auth.Tasks.Options;
using Christofel.CtuAuth.Database;
using Christofel.CtuAuth.Extensions;
using Christofel.CtuAuth.JobQueue;
using Christofel.Helpers.JobQueue;
using Christofel.Helpers.ReadOnlyDatabase;
using Christofel.Helpers.Storages;
using Christofel.Management.Commands;
using Christofel.Management.CtuUtils;
using Christofel.Management.Database;
using Christofel.Management.ResendRule;
using Christofel.Management.Slowmode;
using Christofel.OAuth;
using Christofel.Plugins;
using Christofel.Plugins.Lifetime;
using Christofel.Plugins.Runtime;
using Christofel.Remora.Responders;
using Kos;
using Kos.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remora.Commands.Extensions;
using Remora.Discord.Gateway.Extensions;
using Remora.Extensions.Options.Immutable;
using Usermap;
using Usermap.Extensions;

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

                // Ctu auth
                .AddCtuAuthProcess()
                .AddDefaultCtuAuthProcess()

                .AddChristofelDbContextFactory<ApiCacheContext>(State.Configuration)
                .Configure<WarnOptions>(State.Configuration.GetSection("Auth"))
                .Configure<EditInteractionOptions>(State.Configuration.GetSection("Auth"))
                .AddSingleton<IJobQueue<CtuAuthRoleAssign>, CtuAuthRoleAssignProcessor>()
                .AddSingleton<IJobQueue<CtuAuthNicknameSet>, CtuAuthNicknameSetProcessor>()
                .AddSingleton<IJobQueue<CtuAuthWarnMessage>, CtuAuthWarnMessageProcessor>()
                .AddSingleton<IJobQueue<CtuAuthInteractionEdit>, CtuAuthInteractionProcessor>()
                .AddSingleton<CtuAuthRoleAssignService>()

                // Oauth client
                .AddSingleton<CtuOauthHandler>()
                .Configure<CtuOauthOptions>("CtuFel", State.Configuration.GetSection("Oauth:CtuClient"))
                .AddSingleton<ClientCredentialsToken>()

                // kos api, usermap api
                .AddKosApi
                (
                    async (p, ct) =>
                    {
                        var credentialsToken = p.GetRequiredService<ClientCredentialsToken>();

                        await credentialsToken.MakeSureTokenValid(ct);

                        if (credentialsToken.AccessToken is null)
                        {
                            throw new InvalidOperationException("The client credentials token is null.");
                        }

                        return credentialsToken.AccessToken;
                    },
                    lifetime: ServiceLifetime.Scoped
                )
                .AddUsermapApi
                (
                    p =>
                    {
                        var credentialsToken = p.GetRequiredService<ClientCredentialsToken>();

                        credentialsToken.MakeSureTokenValid().Wait();

                        if (credentialsToken.AccessToken is null)
                        {
                            throw new InvalidOperationException("The client credentials token is null.");
                        }

                        return credentialsToken.AccessToken;
                    },
                    lifetime: ServiceLifetime.Scoped
                )
                .Configure<UsermapApiOptions>(State.Configuration.GetSection("Apis:Usermap"))
                .Configure<KosApiOptions>(State.Configuration.GetSection("Apis:Kos"))

                // Misc
                .AddSingleton(_lifetimeHandler.LifetimeSpecific)

                // Configurations
                .Configure<ResendRuleOptions>(State.Configuration.GetSection("Management:Resend"))
                .Configure<UsersOptions>(State.Configuration.GetSection("Management:Users"))
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