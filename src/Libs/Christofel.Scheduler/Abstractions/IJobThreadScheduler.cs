//
//  IJobThreadScheduler.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Remora.Results;

namespace Christofel.Scheduler.Abstractions
{
    /// <summary>
    /// Schedules job execution to the thread.
    /// </summary>
    public interface IJobThreadScheduler
    {
        /// <summary>
        /// Execute task when possible.
        /// </summary>
        /// <param name="executionTask">The task to be executed.</param>
        /// <param name="jobContext">The context of the job.</param>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A result that may not have succeeded.</returns>
        public Task<Result> ScheduleExecutionAsync
        (
            Func<IJobContext, CancellationToken, Task> executionTask,
            IJobContext jobContext,
            CancellationToken ct = default
        );
    }
}