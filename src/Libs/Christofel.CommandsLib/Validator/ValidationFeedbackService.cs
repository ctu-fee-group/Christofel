using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace Christofel.CommandsLib.Validator
{
    public class ValidationFeedbackService
    {
        private readonly FeedbackService _feedbackService;

        public ValidationFeedbackService(FeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        public Task<Result<IMessage>> SendContextualValidationError(
            IReadOnlyList<ValidationFailure> validationFailures, CancellationToken ct = default)
        {
            var embed = new Embed(
                "Validation errors",
                EmbedType.Rich,
                Fields: validationFailures.Select(GetField).ToList(),
                Colour: _feedbackService.Theme.FaultOrDanger
            );

            return _feedbackService.SendContextualEmbedAsync(embed, ct);
        }

        private IEmbedField GetField(ValidationFailure validationFailure)
        {
            return new EmbedField(validationFailure.PropertyName, validationFailure.ErrorMessage);
        }
    }
}