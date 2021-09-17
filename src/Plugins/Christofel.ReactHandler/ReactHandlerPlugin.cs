//
//   ReactHandlerPlugin.cs
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
using Christofel.Plugins.Lifetime;
using Christofel.Plugins.Runtime;
using Christofel.ReactHandler.Commands;
using Christofel.ReactHandler.Database;
using Christofel.ReactHandler.Responders;
using Christofel.Remora.Responders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remora.Commands.Extensions;
using Remora.Discord.Gateway.Extensions;

namespace Christofel.ReactHandler
{
    /// <summary>
    /// Plugin for handling commands that specify what messages to react to and reacting to these messages.
    /// </summary>
    public class ReactHandlerPlugin : ChristofelDIPlugin
    {
        private readonly PluginLifetimeHandler _lifetimeHandler;
        private ILogger<ReactHandlerPlugin>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReactHandlerPlugin"/> class.
        /// </summary>
        public ReactHandlerPlugin()
        {
            _lifetimeHandler = new PluginLifetimeHandler
            (
                DefaultHandleError(() => _logger),
                DefaultHandleStopRequest(() => _logger)
            );
        }

        /// <inheritdoc />
        public override string Name => "Christofel.ReactHandler";

        /// <inheritdoc />
        public override string Description =>
            "Plugin for handling reacts on messages, marking messages to assign channels/roles on react";

        /// <inheritdoc />
        public override string Version => "v1.0.0";

        /// <inheritdoc />
        protected override LifetimeHandler LifetimeHandler => _lifetimeHandler;

        /// <inheritdoc />
        protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddDiscordState(State)
                .AddSingleton<PluginResponder>()
                .AddChristofelCommands()
                .AddCommandGroup<HandleReactCommands>()
                .AddResponder<DeleteReactHandlerResponder>()
                .AddResponder<HandleReactResponder>()
                .AddSingleton(_lifetimeHandler.LifetimeSpecific)
                .AddChristofelDbContextFactory<ReactHandlerContext>(State.Configuration)
                .AddTransient
                (
                    p =>
                        p.GetRequiredService<IDbContextFactory<ReactHandlerContext>>().CreateDbContext()
                )
                .AddReadOnlyDbContext<ReactHandlerContext>()
                .Configure<BotOptions>(State.Configuration.GetSection("Bot"));
        }

        /// <inheritdoc />
        protected override Task InitializeServices
        (
            IServiceProvider services,
            CancellationToken token = default
        )
        {
            _logger = services.GetRequiredService<ILogger<ReactHandlerPlugin>>();
            ((PluginContext)Context).PluginResponder = services.GetRequiredService<PluginResponder>();
            return Task.CompletedTask;
        }
    }
}