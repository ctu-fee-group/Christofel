//
//  CoursesPlugin.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.Plugins;
using Christofel.CommandsLib;
using Christofel.CommandsLib.Extensions;
using Christofel.Courses.Commands;
using Christofel.CoursesLib.Extensions;
using Christofel.OAuth;
using Christofel.Plugins.Lifetime;
using Christofel.Plugins.Runtime;
using Christofel.Remora.Responders;
using Kos;
using Kos.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remora.Commands.Extensions;
using Remora.Extensions.Options.Immutable;

namespace Christofel.Courses
{
    /// <summary>
    /// Hello world plugin that responds pong to /ping command.
    /// </summary>
    public class CoursesPlugin : ChristofelDIPlugin
    {
        private readonly PluginLifetimeHandler _lifetimeHandler;
        private ILogger<CoursesPlugin>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoursesPlugin"/> class.
        /// </summary>
        public CoursesPlugin()
        {
            _lifetimeHandler = new PluginLifetimeHandler
            (
                DefaultHandleError(() => _logger),
                DefaultHandleStopRequest(() => _logger)
            );
        }

        /// <inheritdoc />
        public override string Name => "Christofel.Courses";

        /// <inheritdoc />
        public override string Description => "Plugin for managing channels for courses, assigning them to users.";

        /// <inheritdoc />
        public override string Version => "v0.0.1";

        /// <inheritdoc />
        protected override LifetimeHandler LifetimeHandler => _lifetimeHandler;

        /// <inheritdoc />
        protected override IServiceCollection ConfigureServices
            (IServiceCollection serviceCollection)
            => serviceCollection
                .AddDiscordState(State)
                .AddChristofelDatabase(State)
                .AddSingleton<PluginResponder>()
                .AddChristofelCommands()
                .AddCommandGroup<CoursesAdminCommands>()
                .AddCommandGroup<CoursesCommands>()
                .AddSingleton<CtuOauthHandler>()
                .Configure<CtuOauthOptions>("Ctu", State.Configuration.GetSection("Oauth:CtuClient"))
                .AddSingleton<ClientCredentialsToken>()
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
                .AddCourses(State.Configuration)
                .AddSingleton(_lifetimeHandler.LifetimeSpecific)
                .Configure<BotOptions>(State.Configuration.GetSection("Bot"))
                .Configure<KosApiOptions>(State.Configuration.GetSection("Apis:Kos"));

        /// <inheritdoc />
        protected override Task InitializeServices
            (IServiceProvider services, CancellationToken token = default)
        {
            _logger = services.GetRequiredService<ILogger<CoursesPlugin>>();
            ((PluginContext)Context).PluginResponder = services.GetRequiredService<PluginResponder>();
            return Task.CompletedTask;
        }
    }
}