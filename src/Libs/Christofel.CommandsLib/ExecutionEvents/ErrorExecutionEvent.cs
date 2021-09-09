//
//   ErrorExecutionEvent.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Remora.Commands.Results;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Services;
using Remora.Results;

namespace Christofel.CommandsLib.ExecutionEvents
{
    public class ErrorExecutionEvent : IPostExecutionEvent
    {
        private readonly ILogger _logger;

        public ErrorExecutionEvent(ILogger<ErrorExecutionEvent> logger)
        {
            _logger = logger;
        }

        public Task<Result> AfterExecutionAsync
        (
            ICommandContext context,
            IResult commandResult,
            CancellationToken ct = new CancellationToken()
        )
        {
            if (!commandResult.IsSuccess && commandResult.Error is (not null and not CommandNotFoundError))
            {
                switch (commandResult.Error)
                {
                    case ExceptionError exceptionError:
                        _logger.LogError(exceptionError.Exception, "Command returned exception error");
                        break;
                    default:
                        _logger.LogError($"Command returned an error: {commandResult.Error.Message}");
                        break;
                }
            }

            return Task.FromResult(Result.FromSuccess());
        }
    }
}