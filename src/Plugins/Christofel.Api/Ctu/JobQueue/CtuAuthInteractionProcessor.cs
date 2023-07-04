//
//  CtuAuthInteractionProcessor.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Christofel.BaseLib.Extensions;
using Christofel.Helpers.JobQueue;
using Christofel.Plugins.Lifetime;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;

namespace Christofel.Api.Ctu.JobQueue;

/// <summary>
/// Processor of role assigns, works on different thread.
/// </summary>
/// <remarks>
/// Creates thread only if there is job assigned,
/// if there isn't, the thread is freed (thread pool is used).
/// </remarks>
public class CtuAuthInteractionProcessor : ThreadPoolJobQueue<CtuAuthInteractionEdit>
{
    private readonly ILogger _logger;
    private readonly ILifetime _pluginLifetime;
    private readonly IDiscordRestInteractionAPI _interactionApi;
    private readonly IDiscordRestOAuth2API _oauth2Api;

    /// <summary>
    /// Initializes a new instance of the <see cref="CtuAuthInteractionProcessor"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="pluginLifetime">The lifetime of the current plugin.</param>
    /// <param name="interactionApi">The interaction api.</param>
    /// <param name="oauth2Api">The oauth api.</param>
    public CtuAuthInteractionProcessor
    (
        ILogger<CtuAuthInteractionProcessor> logger,
        ICurrentPluginLifetime pluginLifetime,
        IDiscordRestInteractionAPI interactionApi,
        IDiscordRestOAuth2API oauth2Api
    )
        : base(pluginLifetime, logger)
    {
        _pluginLifetime = pluginLifetime;
        _interactionApi = interactionApi;
        _oauth2Api = oauth2Api;
        _interactionApi = interactionApi;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ProcessAssignJob(CtuAuthInteractionEdit assignJob)
    {
        var appResult = await _oauth2Api.GetCurrentBotApplicationInformationAsync(_pluginLifetime.Stopping);

        if (!appResult.IsDefined(out var app))
        {
            _logger.LogResultError(appResult, "Cannot obtain app information");
            return;
        }

        // Result is not important, if this doesn't work, do not log, this is going to happen often.
        // The problem is that users may login after more than 15 minutes when the interaction is no longer
        // active.
        var unused = await _interactionApi.EditOriginalInteractionResponseAsync
        (
            app.ID,
            assignJob.Token,
            assignJob.EditedMessage,
            components: Array.Empty<IMessageComponent>(),
            ct: _pluginLifetime.Stopping
        );
    }
}

/// <summary>
/// The job for <see cref="Christofel.Api.Ctu.JobQueue.CtuAuthInteractionProcessor"/>.
/// </summary>
/// <param name="Token">The token of the interaction.</param>
public record CtuAuthInteractionEdit(string Token, string EditedMessage);