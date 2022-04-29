//
//  ConstructDatabasePlugin.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.Plugins;
using Christofel.Plugins;
using Christofel.Plugins.Lifetime;
using Christofel.Remora.Responders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;

namespace Christofel.ConstructDatabase
{
    /// <summary>
    /// The plugin for constructing database roles.
    /// </summary>
    public class ConstructDatabasePlugin : ChristofelDIPlugin
    {
        private readonly PluginLifetimeHandler _lifetimeHandler;
        private ILogger? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstructDatabasePlugin"/> class.
        /// </summary>
        public ConstructDatabasePlugin()
        {
            _lifetimeHandler = new PluginLifetimeHandler
            (
                DefaultHandleError(() => _logger),
                DefaultHandleStopRequest(() => _logger)
            );
        }

        /// <inheritdoc />
        public override string Name => "Christofel.ConstructDatabase";

        /// <inheritdoc />
        public override string Description => "Plugin for adding roles to the database.";

        /// <inheritdoc />
        public override string Version => "v1.0.0";

        /// <inheritdoc />
        protected override LifetimeHandler LifetimeHandler => _lifetimeHandler;

        /// <inheritdoc />
        protected override IServiceCollection ConfigureServices
            (IServiceCollection serviceCollection) => serviceCollection
            .AddDiscordState(State)
            .AddSingleton<PluginResponder>()
            .AddStateful<DatabaseBuilder>()
            .AddSingleton(_lifetimeHandler.LifetimeSpecific)
            .Configure<BotOptions>(State.Configuration.GetSection("Bot"));

        /// <inheritdoc />
        protected override Task InitializeServices
            (IServiceProvider services, CancellationToken token = default)
        {
            _logger = services.GetRequiredService<ILogger<ConstructDatabasePlugin>>();
            ((PluginContext)Context).PluginResponder = services.GetRequiredService<PluginResponder>();
            return Task.CompletedTask;
        }
    }
}