//
//   ErrorExecutionEvent.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Extensions;
using Microsoft.Extensions.Logging;
using Remora.Commands.Results;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Services;
using Remora.Results;

namespace Christofel.CommandsLib.ExecutionEvents
{
    /// <summary>
    /// Event logging errors into given logger.
    /// </summary>
    public class ErrorExecutionEvent : IPostExecutionEvent
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorExecutionEvent"/> class.
        /// </summary>
        /// <param name="logger">The logger to log errors with.</param>
        public ErrorExecutionEvent(ILogger<ErrorExecutionEvent> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public Task<Result> AfterExecutionAsync
        (
            ICommandContext context,
            IResult commandResult,
            CancellationToken ct = default
        )
        {
            if (!commandResult.IsSuccess && commandResult.Error is not null and not CommandNotFoundError)
            {
                _logger.LogResultError(commandResult, $"Command executed by <@{context.User.ID}> returned an error");
            }

            return Task.FromResult(Result.FromSuccess());
        }
    }
}