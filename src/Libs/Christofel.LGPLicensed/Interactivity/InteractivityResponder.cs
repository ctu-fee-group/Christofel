using Christofel.CommandsLib;
using Christofel.CommandsLib.Attributes;
using Humanizer;
using Microsoft.Extensions.Options;
using OneOf;
using Remora.Commands.Services;
using Remora.Commands.Tokenization;
using Remora.Commands.Trees;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway.Responders;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.LGPLicensed.Interactivity;

/// <inheritdoc />
/// This code is from Remora.Discord by @Nihlus
/// see https://github.com/Remora/Remora.Discord/blob/main/Remora.Discord.Interactivity/Responders/InteractivityResponder.cs
public sealed class InteractivityResponder : IResponder<IInteractionCreate>
{
    private readonly ContextInjectionService _contextInjectionService;
    private readonly ExecutionEventCollectorService _eventCollector;
    private readonly IDiscordRestInteractionAPI _interactionAPI;
    private readonly IServiceProvider _services;
    private readonly InteractivityResponderOptions _options;
    private readonly CommandService _commandService;

    private readonly TokenizerOptions _tokenizerOptions;
    private readonly TreeSearchOptions _treeSearchOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractivityResponder"/> class.
    /// </summary>
    /// <param name="commandService">The command service.</param>
    /// <param name="options">The responder options.</param>
    /// <param name="interactionAPI">The interaction API.</param>
    /// <param name="services">The available services.</param>
    /// <param name="contextInjectionService">The context injection service.</param>
    /// <param name="eventCollector">The event collector.</param>
    /// <param name="tokenizerOptions">The tokenizer options.</param>
    /// <param name="treeSearchOptions">The tree search options.</param>
    public InteractivityResponder
    (
        CommandService commandService,
        IOptions<InteractivityResponderOptions> options,
        IDiscordRestInteractionAPI interactionAPI,
        IServiceProvider services,
        ContextInjectionService contextInjectionService,
        ExecutionEventCollectorService eventCollector,
        IOptions<TokenizerOptions> tokenizerOptions,
        IOptions<TreeSearchOptions> treeSearchOptions
    )
    {
        _services = services;
        _contextInjectionService = contextInjectionService;
        _eventCollector = eventCollector;
        _interactionAPI = interactionAPI;
        _commandService = commandService;
        _options = options.Value;

        _tokenizerOptions = tokenizerOptions.Value;
        _treeSearchOptions = treeSearchOptions.Value;
    }

    /// <inheritdoc />
    public async Task<Result> RespondAsync(IInteractionCreate gatewayEvent, CancellationToken ct = default)
    {
        if (gatewayEvent.Type is not (InteractionType.MessageComponent or InteractionType.ModalSubmit))
        {
            return Result.FromSuccess();
        }

        if (!gatewayEvent.Data.IsDefined(out var data))
        {
            return new InvalidOperationError("Component or modal interaction without data received. Bug?");
        }

        var context = new InteractionContext(gatewayEvent);
        _contextInjectionService.Context = context;

        return data.TryPickT1(out var componentData, out var remainder)
            ? await HandleComponentInteractionAsync(context, componentData, ct)
            : remainder.TryPickT1(out var modalSubmitData, out _)
                ? await HandleModalInteractionAsync(context, modalSubmitData, ct)
                : Result.FromSuccess();
    }

    private async Task<Result> HandleComponentInteractionAsync
    (
        InteractionContext context,
        IMessageComponentData data,
        CancellationToken ct = default
    )
    {
        if (!data.CustomID.StartsWith(Constants.InteractivityPrefix))
        {
            // Not a component we handle
            return Result.FromSuccess();
        }
        
        var isSelectMenu = data.ComponentType is ComponentType.StringSelect
            or ComponentType.UserSelect
            or ComponentType.RoleSelect
            or ComponentType.MentionableSelect
            or ComponentType.ChannelSelect;

        if (isSelectMenu && !data.Values.HasValue)
        {
            if (!data.Values.HasValue)
            {
                return new InvalidOperationError("The interaction did not contain any selected values.");
            }
        }

        var commandPath = data.CustomID[Constants.InteractivityPrefix.Length..][2..]
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var buildParameters = data.ComponentType switch
        {
            ComponentType.Button => new Dictionary<string, IReadOnlyList<string>>(),
            ComponentType.StringSelect => Result<IReadOnlyDictionary<string, IReadOnlyList<string>>>.FromSuccess
            (
                new Dictionary<string, IReadOnlyList<string>>
                {
                    { "values", data.Values.Value },
                }
            ),
            _ => new InvalidOperationError("An unsupported component type was encountered.")
        };

        if (!buildParameters.IsSuccess)
        {
            return (Result)buildParameters;
        }

        var parameters = buildParameters.Entity;

        return await TryExecuteInteractionCommandAsync(context, commandPath, parameters, ct);
    }

    private async Task<Result> HandleModalInteractionAsync
    (
        InteractionContext context,
        IModalSubmitData data,
        CancellationToken ct = default
    )
    {
        if (!data.CustomID.StartsWith(Constants.InteractivityPrefix))
        {
            // Not a component we handle
            return Result.FromSuccess();
        }

        var commandPath = data.CustomID[Constants.InteractivityPrefix.Length..][2..]
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var parameters = ExtractParameters(data.Components);

        return await TryExecuteInteractionCommandAsync
        (
            context,
            commandPath,
            parameters,
            ct
        );
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> ExtractParameters
    (
        IEnumerable<IPartialMessageComponent> components
    )
    {
        var parameters = new Dictionary<string, IReadOnlyList<string>>();
        foreach (var component in components)
        {
            if (component is IPartialActionRowComponent actionRow)
            {
                if (!actionRow.Components.IsDefined(out var rowComponents))
                {
                    continue;
                }

                var nestedComponents = ExtractParameters(rowComponents);
                foreach (var nestedComponent in nestedComponents)
                {
                    parameters.Add(nestedComponent.Key, nestedComponent.Value);
                }

                continue;
            }

            switch (component)
            {
                case IPartialTextInputComponent textInput:
                {
                    if (!textInput.CustomID.IsDefined(out var id))
                    {
                        continue;
                    }

                    if (!textInput.Value.IsDefined(out var value))
                    {
                        continue;
                    }

                    parameters.Add(id.Replace('-', '_').Camelize(), new[] { value });
                    break;
                }
                case StringSelectComponent selectMenu:
                {
                    var values = selectMenu.Options.Select(op => op.Value).ToList();
                    parameters.Add(selectMenu.CustomID.Replace('-', '_').Camelize(), values);
                    break;
                }
            }
        }

        return parameters;
    }

    private async Task<Result> TryExecuteInteractionCommandAsync
    (
        InteractionContext context,
        IReadOnlyList<string> commandPath,
        IReadOnlyDictionary<string, IReadOnlyList<string>> parameters,
        CancellationToken ct
    )
    {
        string commandString = string.Join(' ', commandPath).Trim(' ') + " " + string.Join
            (' ', parameters.Select(x => $"{x.Value.FirstOrDefault()}"));

        var prepareCommand = await _commandService.TryPrepareCommandAsync
        (
            commandString,
            _services,
            searchOptions: _treeSearchOptions,
            tokenizerOptions: _tokenizerOptions,
            treeName: Constants.InteractivityPrefix,
            ct: ct
        );

        if (!prepareCommand.IsSuccess)
        {
            var preparationError = await _eventCollector.RunPreparationErrorEvents
            (
                _services,
                context,
                prepareCommand,
                ct
            );

            if (!preparationError.IsSuccess)
            {
                return preparationError;
            }

            return (Result)prepareCommand;
        }

        var preparedCommand = prepareCommand.Entity;
        var commandContext = new InteractionCommandContext(context.Interaction, preparedCommand);
        _contextInjectionService.Context = commandContext;

        // Run any user-provided pre-execution events
        var preExecution = await _eventCollector.RunPreExecutionEvents(_services, commandContext, ct);
        if (!preExecution.IsSuccess)
        {
            return preExecution;
        }

        var suppressResponseAttribute = preparedCommand.Command.Node
            .FindCustomAttributeOnLocalTree<SuppressInteractionResponseAttribute>();
        var ephemeralAttribute = preparedCommand.Command.Node
            .FindCustomAttributeOnLocalTree<EphemeralAttribute>();
        var interactionResponseCallbackAttribute = preparedCommand.Command.Node
            .FindCustomAttributeOnLocalTree<InteractionCallbackTypeAttribute>();

        var shouldSendResponse = !(suppressResponseAttribute?.Suppress ?? _options.SuppressAutomaticResponses);

        // ReSharper disable once InvertIf
        if (shouldSendResponse)
        {
            var response = new InteractionResponse
            (
                interactionResponseCallbackAttribute?.InteractionCallbackType ?? InteractionCallbackType.DeferredChannelMessageWithSource,
                new Optional<OneOf<IInteractionMessageCallbackData, IInteractionAutocompleteCallbackData,
                    IInteractionModalCallbackData>>
                (
                    new InteractionMessageCallbackData
                        (Flags: (ephemeralAttribute?.IsEphemeral ?? false) ? MessageFlags.Ephemeral : 0)
                )
            );
            var createResponse = await _interactionAPI.CreateInteractionResponseAsync
            (
                context.Interaction.ID,
                context.Interaction.Token,
                response,
                ct: ct
            );

            if (!createResponse.IsSuccess)
            {
                return createResponse;
            }
            
            context.HasRespondedToInteraction = true;
            commandContext.HasRespondedToInteraction = true;
        }

        // Run the actual command
        var executeResult = await _commandService.TryExecuteAsync
        (
            preparedCommand,
            _services,
            ct
        );

        if (!executeResult.IsSuccess)
        {
            return (Result)executeResult;
        }

        return await _eventCollector.RunPostExecutionEvents
        (
            _services,
            commandContext,
            executeResult.IsSuccess ? executeResult.Entity : executeResult,
            ct
        );
    }
}