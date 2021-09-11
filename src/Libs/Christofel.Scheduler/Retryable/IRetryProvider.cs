//
//   IRetryProvider.cs
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
    /// Service for repeating scheduled tasks that have failed.
    /// </summary>
    public interface IRetryProvider
    {
        /// <summary>
        /// Gets how many times should be the command repeated.
        /// </summary>
        public int MaxRepeatCount { get; }

        /// <summary>
        /// Schedules repeat of the task.
        /// </summary>
        /// <param name="jobContext">The context of the current job.</param>
        /// <param name="jobResult">The result of the execution.</param>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A result that may not have succeeded.</returns>
        public Task<Result> ScheduleRepeatAsync(IJobContext jobContext, Result jobResult, CancellationToken ct = default);
    }
}