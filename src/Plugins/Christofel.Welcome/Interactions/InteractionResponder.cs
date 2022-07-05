//
//   InteractionResponder.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using OneOf;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway.Responders;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.Welcome.Interactions;

/// <summary>
/// Responds to interaction create and handles welcome interactions.
/// </summary>
public class InteractionResponder : IResponder<IInteractionCreate>
{
    private readonly ContextInjectionService _contextInjectionService;
    private readonly IDiscordRestInteractionAPI _interactionApi;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractionResponder"/> class.
    /// </summary>
    /// <param name="contextInjectionService">The injection service.</param>
    /// <param name="interactionApi">The interaction api.</param>
    /// <param name="serviceProvider">The dependency injection service provider.</param>
    public InteractionResponder
    (
        ContextInjectionService contextInjectionService,
        IDiscordRestInteractionAPI interactionApi,
        IServiceProvider serviceProvider
    )
    {
        _contextInjectionService = contextInjectionService;
        _interactionApi = interactionApi;
        _serviceProvider = serviceProvider;

    }

    /// <inheritdoc />
    public async Task<Result> RespondAsync(IInteractionCreate gatewayEvent, CancellationToken ct = default)
    {
        if (gatewayEvent.Type != InteractionType.MessageComponent)
        {
            return Result.FromSuccess();
        }

        if (!gatewayEvent.Data.IsDefined(out var data))
        {
            return Result.FromSuccess();
        }

        if (!data.TryPickT1(out var messageData, out _))
        {
            return Result.FromSuccess();
        }

        if (!messageData.CustomID.StartsWith(Constants.ChristofelPrefix))
        {
            return Result.FromSuccess();
        }

        var contextResult = gatewayEvent.CreateContext();
        if (!contextResult.IsDefined(out var context))
        {
            return Result.FromError(contextResult);
        }
        _contextInjectionService.Context = context;

        var interactions = _serviceProvider.GetRequiredService<WelcomeInteractions>();

        var path = messageData.CustomID[(Constants.ChristofelPrefix.Length + 2)..].Split(' ');

        if (path[0] != "welcome" || path.Length < 2)
        {
            return Result.FromSuccess();
        }

        var interactionResponse = await _interactionApi.CreateInteractionResponseAsync
        (
            gatewayEvent.ID,
            gatewayEvent.Token,
            new InteractionResponse
            (
                InteractionCallbackType.DeferredChannelMessageWithSource,
                new Optional<OneOf<IInteractionMessageCallbackData, IInteractionAutocompleteCallbackData,
                    IInteractionModalCallbackData>>(new InteractionMessageCallbackData(Flags: MessageFlags.Ephemeral))
            ),
            ct: ct
        );

        if (!interactionResponse.IsSuccess)
        {
            return interactionResponse;
        }

        string? lang;
        switch (path[1])
        {
            case "show":
                if (path.Length < 3)
                {
                    if (!messageData.Values.IsDefined(out var values))
                    {
                        return Result.FromSuccess();
                    }

                    lang = values.First();
                }
                else
                {
                    lang = path[2];
                }

                return await interactions.HandleShowAsync(lang, ct);
            case "auth":
                if (path.Length < 3)
                {
                    return Result.FromSuccess();
                }

                lang = path[2];
                return await interactions.HandleAuthButtonAsync(lang, ct);
        }

        return Result.FromSuccess();
    }
}