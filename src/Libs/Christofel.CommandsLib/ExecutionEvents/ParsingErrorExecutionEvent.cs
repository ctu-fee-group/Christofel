//
//   ParsingErrorExecutionEvent.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Remora.Commands.Results;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Commands.Services;
using Remora.Results;

namespace Christofel.CommandsLib.ExecutionEvents
{
    /// <summary>
    /// Event catching parsing errors for notifying the user about what happened.
    /// </summary>
    public class ParsingErrorExecutionEvent : IPostExecutionEvent
    {
        private readonly ICommandContext _commandContext;
        private readonly FeedbackService _feedbackService;
        private readonly IDiscordRestInteractionAPI _interactionApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParsingErrorExecutionEvent"/> class.
        /// </summary>
        /// <param name="interactionApi">The interaction api.</param>
        /// <param name="commandContext">The context of the current command.</param>
        /// <param name="feedbackService">The feedback service to respond with.</param>
        public ParsingErrorExecutionEvent
        (
            IDiscordRestInteractionAPI interactionApi,
            ICommandContext commandContext,
            FeedbackService feedbackService
        )
        {
            _interactionApi = interactionApi;
            _feedbackService = feedbackService;
            _commandContext = commandContext;
        }

        /// <inheritdoc />
        public async Task<Result> AfterExecutionAsync
        (
            ICommandContext context,
            IResult commandResult,
            CancellationToken ct = default
        )
        {
            if (!commandResult.IsSuccess &&
                commandResult.Error is ParameterParsingError parsingError)
            {
                var message = parsingError.Message;
                if (commandResult.Inner?.Inner?.Inner?.Error is not null)
                {
                    message += "\n" + commandResult.Inner.Inner.Inner.Error.Message;
                }

                if (_commandContext is InteractionContext interactionContext)
                {
                    var result = await _interactionApi.CreateInteractionResponseAsync
                    (
                        interactionContext.ID,
                        interactionContext.Token,
                        new InteractionResponse
                        (
                            InteractionCallbackType.ChannelMessageWithSource,
                            new InteractionCallbackData
                            (
                                Content: message,
                                Flags: InteractionCallbackDataFlags.Ephemeral
                            )
                        ),
                        ct
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