using Christofel.CommandsLib;
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
        if (gatewayEvent.Type is not(InteractionType.MessageComponent or InteractionType.ModalSubmit))
        {
            return Result.FromSuccess();
        }

        if (!gatewayEvent.Data.IsDefined(out var data))
        {
            return new InvalidOperationError("Component or modal interaction without data received. Bug?");
        }

        var createContext = gatewayEvent.CreateContext();
        if (!createContext.IsSuccess)
        {
            return (Result)createContext;
        }

        var context = createContext.Entity;
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

        if (data.ComponentType is ComponentType.SelectMenu)
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
            ComponentType.SelectMenu => Result<IReadOnlyDictionary<string, IReadOnlyList<string>>>.FromSuccess
            (
                new Dictionary<string, IReadOnlyList<string>>
                {
                    { "values", data.Values.Value }
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
                case IPartialSelectMenuComponent selectMenu:
                {
                    if (!selectMenu.CustomID.IsDefined(out var id))
                    {
                        continue;
                    }

                    if (!selectMenu.Options.IsDefined(out var options))
                    {
                        continue;
                    }

                    var values = options.Where(op => op.Value.HasValue).Select(op => op.Value.Value).ToList();

                    parameters.Add(id.Replace('-', '_').Camelize(), values);
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
        // Run any user-provided pre-execution events
        var preExecution = await _eventCollector.RunPreExecutionEvents(_services, context, ct);
        if (!preExecution.IsSuccess)
        {
            return preExecution;
        }

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
            return (Result)prepareCommand;
        }

        var preparedCommand = prepareCommand.Entity;

        var suppressResponseAttribute = preparedCommand.Command.Node
            .FindCustomAttributeOnLocalTree<SuppressInteractionResponseAttribute>();
        var ephemeralAttribute = preparedCommand.Command.Node
            .FindCustomAttributeOnLocalTree<EphemeralAttribute>();

        var shouldSendResponse = !(suppressResponseAttribute?.Suppress ?? _options.SuppressAutomaticResponses);

        // ReSharper disable once InvertIf
        if (shouldSendResponse)
        {
            var response = new InteractionResponse
            (
                InteractionCallbackType.DeferredChannelMessageWithSource,
                new Optional<OneOf<IInteractionMessageCallbackData, IInteractionAutocompleteCallbackData,
                    IInteractionModalCallbackData>>
                (
                    new InteractionMessageCallbackData
                        (Flags: (ephemeralAttribute?.IsEphemeral ?? false) ? MessageFlags.Ephemeral : 0)
                )
            );
            var createResponse = await _interactionAPI.CreateInteractionResponseAsync
            (
                context.ID,
                context.Token,
                response,
                ct: ct
            );

            if (!createResponse.IsSuccess)
            {
                return createResponse;
            }
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
            context,
            executeResult.IsSuccess ? executeResult.Entity : executeResult,
            ct
        );
    }
}