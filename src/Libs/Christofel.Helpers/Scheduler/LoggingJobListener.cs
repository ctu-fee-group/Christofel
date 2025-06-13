//
//   LoggingJobListener.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Christofel.Scheduling;
using Christofel.Scheduling.Extensions;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.Helpers.Scheduler
{
    /// <summary>
    /// Job listener that logs error results of jobs.
    /// </summary>
    public class LoggingJobListener : IJobListener
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingJobListener"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public LoggingJobListener(ILogger<LoggingJobListener> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public ValueTask<Result> BeforeExecutionAsync
            (IJobContext context, CancellationToken ct = default) => ValueTask.FromResult(Result.FromSuccess());

        /// <inheritdoc />
        public ValueTask<Result> AfterExecutionAsync
            (IJobContext context, Result jobResult, CancellationToken ct = default)
        {
            if (!jobResult.IsSuccess)
            {
                _logger.LogResult(jobResult, "Job returned an error.");
            }

            return ValueTask.FromResult(Result.FromSuccess());
        }
    }
}