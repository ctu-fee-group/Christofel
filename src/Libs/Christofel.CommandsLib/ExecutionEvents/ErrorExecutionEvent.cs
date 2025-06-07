//
//   ErrorExecutionEvent.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Extensions;
using Microsoft.Extensions.Logging;
using Remora.Commands.Results;
using Remora.Commands.Services;
using Remora.Commands.Trees.Nodes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
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
            var user = "Unknown";
            if (context.TryGetUserID(out var userId))
            {
                user = $"<@{userId}>";
            }

            if (!commandResult.IsSuccess && commandResult.Error is not null and not CommandNotFoundError)
            {
                _logger.LogResultError(commandResult, $"Command \"/{GetCommandString(context.Command)}\" executed by {user} returned an error");
            }

            return Task.FromResult(Result.FromSuccess());
        }

        private string GetCommandString(PreparedCommand command)
        {
            var keys = new List<string>();
            keys.Add(command.Command.Node.Key);

            var parentNode = command.Command.Node.Parent;

            while (parentNode is IChildNode childNode)
            {
                keys.Add(childNode.Key);
                parentNode = childNode.Parent;
            }

            return string.Join(' ', Enumerable.Reverse(keys)) + " " + string.Join(' ', command.Parameters);
        }
    }
}