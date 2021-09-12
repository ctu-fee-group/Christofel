//
//  LoggerExtensions.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.Scheduler.Extensions
{
    /// <summary>
    /// Defines extension methods for the type <see cref="ILogger"/>.
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Logs the given result into the given logger.
        /// </summary>
        /// <param name="logger">The logger to log with.</param>
        /// <param name="result">The result to be logged.</param>
        /// <param name="message">The message to begin with.</param>
        public static void LogResult(this ILogger logger, IResult result, string message)
        {
            if (result.IsSuccess)
            {
                return;
            }

            switch (result.Error)
            {
                case ExceptionError exeptionError:
                    logger.LogError(exeptionError.Exception, message);
                    break;
                case AggregateError aggregateError:
                    foreach (var errorResult in aggregateError.Errors)
                    {
                        logger.LogResult(errorResult, message);
                    }

                    break;
                default:
                    logger.LogError("{Message} {Error}", message, result.Error?.Message);
                    break;
            }
        }
    }
}