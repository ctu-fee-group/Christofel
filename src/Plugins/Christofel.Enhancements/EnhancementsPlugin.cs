//
//   EnhancementsPlugin.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.Plugins;
using Christofel.CommandsLib.Extensions;
using Christofel.Enhancements.AutoPin;
using Christofel.Enhancements.CustomVoice;
using Christofel.Enhancements.Teleport;
using Christofel.Helpers.Permissions;
using Christofel.Helpers.Storages;
using Christofel.Plugins.Lifetime;
using Christofel.Remora.Responders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remora.Commands.Extensions;
using Remora.Discord.Gateway.Extensions;

namespace Christofel.Enhancements;

/// <summary>
/// The enhancements plugin.
/// </summary>
public class EnhancementsPlugin : ChristofelDIPlugin
{
    private readonly PluginLifetimeHandler _lifetimeHandler;
    private ILogger<EnhancementsPlugin>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnhancementsPlugin"/> class.
    /// </summary>
    public EnhancementsPlugin()
    {
        _lifetimeHandler = new PluginLifetimeHandler
        (
            DefaultHandleError(() => _logger),
            DefaultHandleStopRequest(() => _logger)
        );
    }

    /// <inheritdoc />
    public override string Name => "Christofel.Enhancements";

    /// <inheritdoc />
    public override string Description => "Plugin for user experience enhancements.";

    /// <inheritdoc />
    public override string Version => "v1.0.0";

    /// <inheritdoc />
    protected override LifetimeHandler LifetimeHandler => _lifetimeHandler;

    /// <inheritdoc />
    protected override IServiceCollection ConfigureServices
        (IServiceCollection serviceCollection)
    {
        // Teleport setup
        serviceCollection
            .Configure<TeleportOptions>(State.Configuration.GetSection("Enhancements:Teleport"))
            .AddCommandTree()
            .WithCommandGroup<TeleportCommandGroup>();

        // Custom voice setup
        serviceCollection
            .AddSingleton<IThreadSafeStorage<CustomVoiceChannel>, ThreadSafeListStorage<CustomVoiceChannel>>()
            .Configure<CustomVoiceOptions>(State.Configuration.GetSection("Enhancements:CustomVoice"))
            .AddResponder<CustomVoiceResponder>()
            .AddScoped<CustomVoiceService>()
            .AddScoped<MemberPermissionResolver>()
            .AddCommandTree()
            .WithCommandGroup<CustomVoiceCommandGroup>();

        // Auto pin setup
        serviceCollection
            .Configure<AutoPinOptions>(State.Configuration.GetSection("Enhancements:AutoPin"))
            .AddResponder<AutoPinResponder>();

        return serviceCollection
            .AddDiscordState(State)
            .AddSingleton<PluginResponder>()
            .AddChristofelCommands()
            .AddSingleton(_lifetimeHandler.LifetimeSpecific)
            .Configure<BotOptions>(State.Configuration.GetSection("Bot"));
    }

    /// <inheritdoc />
    protected override Task InitializeServices
        (IServiceProvider services, CancellationToken token = default)
    {
        _logger = services.GetRequiredService<ILogger<EnhancementsPlugin>>();
        ((PluginContext)Context).PluginResponder = services.GetRequiredService<PluginResponder>();
        return Task.CompletedTask;
    }
}