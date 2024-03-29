//
//   WelcomePlugin.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.Plugins;
using Christofel.CommandsLib.Extensions;
using Christofel.Plugins.Lifetime;
using Christofel.Remora.Responders;
using Christofel.Welcome.Commands;
using Christofel.Welcome.Interactions;
using Christofel.Welcome.Responders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remora.Commands.Extensions;
using Remora.Discord.Gateway.Extensions;

namespace Christofel.Welcome;

/// <summary>
/// A plugin for handling welcome message interactions.
/// </summary>
public class WelcomePlugin : ChristofelDIPlugin
{
    private readonly PluginLifetimeHandler _lifetimeHandler;
    private ILogger<WelcomePlugin>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WelcomePlugin"/> class.
    /// </summary>
    public WelcomePlugin()
    {
        _lifetimeHandler = new PluginLifetimeHandler
        (
            DefaultHandleError(() => _logger),
            DefaultHandleStopRequest(() => _logger)
        );
    }

    /// <inheritdoc />
    public override string Name => "Christofel.Welcome";

    /// <inheritdoc />
    public override string Description => "Plugin for sending and handling the welcome message.";

    /// <inheritdoc />
    public override string Version => "v1.0.0";

    /// <inheritdoc />
    protected override LifetimeHandler LifetimeHandler => _lifetimeHandler;

    /// <inheritdoc />
    protected override IServiceCollection ConfigureServices
        (IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddCommandTree()
            .WithCommandGroup<WelcomeCommands>();

        serviceCollection
            .AddResponder<InteractionResponder>()
            .AddScoped<WelcomeInteractions>()
            .AddTransient<WelcomeMessage>();

        serviceCollection
            .AddResponder<WelcomeResponder>();

        return serviceCollection
            .AddDiscordState(State)
            .AddSingleton<PluginResponder>()
            .AddChristofelCommands()
            .AddChristofelDatabase(State)
            .AddSingleton(_lifetimeHandler.LifetimeSpecific)
            .Configure<WelcomeOptions>(State.Configuration.GetSection("Welcome"))
            .Configure<UsersOptions>(State.Configuration.GetSection("Management:Users"))
            .Configure<BotOptions>(State.Configuration.GetSection("Bot"));
    }

    /// <inheritdoc />
    protected override Task InitializeServices
        (IServiceProvider services, CancellationToken token = default)
    {
        _logger = services.GetRequiredService<ILogger<WelcomePlugin>>();
        ((PluginContext)Context).PluginResponder = services.GetRequiredService<PluginResponder>();
        return Task.CompletedTask;
    }
}