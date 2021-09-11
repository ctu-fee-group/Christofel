//
//   RetryJobListener.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Christofel.Scheduler.Abstractions;
using Remora.Results;

namespace Christofel.Scheduler.Retryable
{
    /// <summary>
    /// Job event listener that retries repeatable jobs that have failed.
    /// </summary>
    public class RetryJobListener : IJobListener
    {
        /// <inheritdoc />
        public ValueTask<Result> BeforeExecutionAsync(IJobContext context, CancellationToken ct = default) => ValueTask.FromResult<Result>(Result.FromSuccess());

        /// <inheritdoc />
        public async ValueTask<Result> AfterExecutionAsync
            (IJobContext context, Result jobResult, CancellationToken ct = default)
        {
            if (jobResult.IsSuccess)
            {
                return Result.FromSuccess();
            }

            if (context.Job is not IRetryableJob repeatableJob)
            {
                return Result.FromSuccess();
            }

            return await repeatableJob.ExternalRetryProvider.ScheduleRepeatAsync(context, jobResult, ct);
        }
    }
}