//
//   BlockingJob.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Scheduler.Abstractions;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.Scheduler.Example.Jobs
{
    /// <summary>
    /// Job blocking for 10 seconds.
    /// </summary>
    public class BlockingJob : IJob
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockingJob"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public BlockingJob(ILogger<BlockingJob> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<Result> ExecuteAsync(IJobContext jobContext, CancellationToken ct = default)
        {
            _logger.LogInformation("Going to block for 10 seconds");
            await Task.Delay(10000, ct);
            _logger.LogInformation("Blocking job is done");
            return Result.FromSuccess();
        }
    }
}