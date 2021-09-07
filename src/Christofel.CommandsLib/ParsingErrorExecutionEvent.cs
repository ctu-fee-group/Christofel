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

namespace Christofel.CommandsLib
{
    public class ParsingErrorExecutionEvent : IPostExecutionEvent
    {
        private readonly FeedbackService _feedbackService;
        private readonly IDiscordRestInteractionAPI _interactionApi;
        private readonly ICommandContext _commandContext;

        public ParsingErrorExecutionEvent(IDiscordRestInteractionAPI interactionApi, ICommandContext commandContext,
            FeedbackService feedbackService)
        {
            _interactionApi = interactionApi;
            _feedbackService = feedbackService;
            _commandContext = commandContext;
        }

        public async Task<Result> AfterExecutionAsync(ICommandContext context, IResult commandResult,
            CancellationToken ct = new CancellationToken())
        {
            if (!commandResult.IsSuccess &&
                commandResult.Error is ParameterParsingError parsingError)
            {
                if (_commandContext is InteractionContext interactionContext)
                {
                    var result = await _interactionApi.CreateInteractionResponseAsync
                    (interactionContext.ID, interactionContext.Token,
                        new InteractionResponse(InteractionCallbackType.ChannelMessageWithSource,
                            new InteractionCallbackData(Content: parsingError.Message,
                                Flags: InteractionCallbackDataFlags.Ephemeral)), ct);

                    return result;
                }

                var feedbackResult =
                    await _feedbackService.SendContextualErrorAsync(commandResult.Error.Message, ct: ct);

                return feedbackResult.IsSuccess
                    ? Result.FromSuccess()
                    : Result.FromError(feedbackResult);
            }

            return Result.FromSuccess();
        }
    }
}