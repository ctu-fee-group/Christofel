//
//   ValidationErrorHandler.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Services;
using Remora.Results;

namespace Christofel.CommandsLib.Validator
{
    /// <summary>
    /// Execution handler for sending validation error to the user.
    /// </summary>
    public class ValidationErrorHandler : IPostExecutionEvent
    {
        private readonly ValidationFeedbackService _feedbackService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationErrorHandler"/> class.
        /// </summary>
        /// <param name="validationFeedbackService">The feedback service to generate and send embed with validation result.</param>
        /// <param name="logger">The logger.</param>
        public ValidationErrorHandler
        (
            ValidationFeedbackService validationFeedbackService,
            ILogger<ValidationErrorHandler> logger
        )
        {
            _logger = logger;
            _feedbackService = validationFeedbackService;
        }

        /// <inheritdoc />
        public async Task<Result> AfterExecutionAsync
        (
            ICommandContext context,
            IResult commandResult,
            CancellationToken ct = default
        )
        {
            var user = "Unknown";
            if (context.TryGetUserID(out var userId))
            {
                user = $"@{userId}";
            }

            if (!commandResult.IsSuccess && commandResult.Error is ValidationResultError validationResultError)
            {
                _logger.LogWarning
                (
                    $"User {userId} has put in invalid data to command, see errors:\n{validationResultError.Message}"
                );
                var feedbackResult = await _feedbackService.SendContextualValidationError
                    (validationResultError.ValidationFailures, ct);

                return feedbackResult.IsSuccess
                    ? Result.FromSuccess()
                    : Result.FromError(feedbackResult);
            }

            return Result.FromSuccess();
        }
    }
}