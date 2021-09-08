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
        private readonly ValidationFeedbackService _feedbackService;

        public ValidationErrorHandler(ICommandContext context, ValidationFeedbackService validationFeedbackService,
            ILogger<ValidationErrorHandler> logger)
        {
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
                var feedbackResult = await _feedbackService.SendContextualValidationError(validationResultError.ValidationFailures, ct);

                return feedbackResult.IsSuccess
                    ? Result.FromSuccess()
                    : Result.FromError(feedbackResult);
            }

            return Result.FromSuccess();
        }
    }
}