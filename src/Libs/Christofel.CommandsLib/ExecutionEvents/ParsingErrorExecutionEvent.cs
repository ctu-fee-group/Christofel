//
//   ParsingErrorExecutionEvent.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OneOf;
using Remora.Commands.Results;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Commands.Services;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.CommandsLib.ExecutionEvents
{
    /// <summary>
    /// Event catching parsing errors for notifying the user about what happened.
    /// </summary>
    public class ParsingErrorExecutionEvent : IPreparationErrorEvent
    {
        private readonly FeedbackService _feedbackService;
        private readonly IDiscordRestInteractionAPI _interactionApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParsingErrorExecutionEvent"/> class.
        /// </summary>
        /// <param name="interactionApi">The interaction api.</param>
        /// <param name="feedbackService">The feedback service to respond with.</param>
        public ParsingErrorExecutionEvent
        (
            IDiscordRestInteractionAPI interactionApi,
            FeedbackService feedbackService
        )
        {
            _interactionApi = interactionApi;
            _feedbackService = feedbackService;
        }

        /// <inheritdoc />
        public async Task<Result> PreparationFailed
        (
            IOperationContext context,
            IResult preparationResult,
            CancellationToken ct = default
        )
        {
            if (!preparationResult.IsSuccess &&
                preparationResult.Error is ParameterParsingError parsingError)
            {
                var message = parsingError.Message;
                var innerError = preparationResult.Inner?.Inner?.Error;
                if (innerError is AggregateError aggregateError)
                {
                    message += "\n" + string.Join('\n', aggregateError.Errors.Select(x => x.Error!.Message));
                }
                else if (innerError is not null)
                {
                    message += "\n" + innerError.Message;
                }

                if (preparationResult is InteractionContext interactionContext)
                {
                    var result = await _interactionApi.CreateInteractionResponseAsync
                    (
                        interactionContext.Interaction.ID,
                        interactionContext.Interaction.Token,
                        new InteractionResponse
                        (
                            InteractionCallbackType.ChannelMessageWithSource,
                            new Optional<OneOf<IInteractionMessageCallbackData, IInteractionAutocompleteCallbackData,
                                IInteractionModalCallbackData>>
                            (
                                new InteractionMessageCallbackData
                                (
                                    Content: message,
                                    Flags: MessageFlags.Ephemeral
                                )
                            )
                        ),
                        ct: ct
                    );

                    return result;
                }

                var feedbackResult =
                    await _feedbackService.SendContextualErrorAsync(message, ct: ct);

                return feedbackResult.IsSuccess
                    ? Result.FromSuccess()
                    : Result.FromError(feedbackResult);
            }

            return Result.FromSuccess();
        }
    }
}