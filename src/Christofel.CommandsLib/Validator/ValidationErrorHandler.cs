using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Commands.Services;
using Remora.Results;

namespace Christofel.CommandsLib.Validator
{
    public class ValidationErrorHandler : IPostExecutionEvent
    {
        private readonly ILogger _logger;
        private readonly ICommandContext _context;
        private readonly FeedbackService _feedbackService;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IDiscordRestInteractionAPI _interactionApi;

        public ValidationErrorHandler(ICommandContext context, FeedbackService validationFeedbackService,
            ILogger<ValidationErrorHandler> logger, IDiscordRestChannelAPI channelApi,
            IDiscordRestInteractionAPI interactionApi)
        {
            _interactionApi = interactionApi;
            _channelApi = channelApi;
            _logger = logger;
            _context = context;
            _feedbackService = validationFeedbackService;
        }

        public async Task<Result> AfterExecutionAsync(ICommandContext context, IResult commandResult,
            CancellationToken ct = new CancellationToken())
        {
            if (!commandResult.IsSuccess && commandResult.Error is ValidationResultError validationResultError)
            {
                _logger.LogWarning(
                    $"User <@{_context.User.ID}> ({_context.User.Username}#{_context.User.Discriminator}) has put in invalid data to command, see errors:\n{validationResultError.Message}");
                var feedbackResult = await SendValidationEmbed(context,
                    GetValidationEmbed(validationResultError.ValidationFailures), ct);

                return feedbackResult.IsSuccess
                    ? Result.FromSuccess()
                    : Result.FromError(feedbackResult);
            }

            return Result.FromSuccess();
        }

        private Embed GetValidationEmbed(IReadOnlyList<ValidationFailure> validationFailures)
        {
            var embed = new Embed(
                "Validation errors",
                EmbedType.Rich,
                Fields: validationFailures.Select(GetField).ToList(),
                Colour: _feedbackService.Theme.FaultOrDanger
            );

            return embed;
        }

        private IEmbedField GetField(ValidationFailure validationFailure)
        {
            return new EmbedField(validationFailure.PropertyName, validationFailure.ErrorMessage);
        }

        private async Task<Result<IMessage>> SendValidationEmbed(ICommandContext context, Embed embed,
            CancellationToken ct)
        {
            switch (context)
            {
                case MessageContext messageContext:
                {
                    return await _channelApi.CreateMessageAsync
                    (
                        messageContext.ChannelID,
                        embeds: new[] { embed },
                        ct: ct
                    );
                }
                case InteractionContext interactionContext:
                {
                    var result = await _interactionApi.CreateFollowupMessageAsync
                    (
                        interactionContext.ApplicationID,
                        interactionContext.Token,
                        embeds: new[] { embed },
                        ct: ct
                    );

                    if (!result.IsSuccess)
                    {
                        return result;
                    }

                    return result;
                }
                default:
                    throw new InvalidOperationException("Invalid context type");
            }
        }
    }
}