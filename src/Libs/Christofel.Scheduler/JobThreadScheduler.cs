//
//   JobThreadScheduler.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Scheduler.Abstractions;
using Remora.Results;

namespace Christofel.Scheduler
{
    /// <summary>
    /// Thread scheduler using default implementation of <see cref="TaskScheduler"/>.
    /// </summary>
    public class JobThreadScheduler : IJobThreadScheduler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JobThreadScheduler"/> class.
        /// </summary>
        public JobThreadScheduler()
        {
            Scheduler = TaskScheduler.Default;
        }

        /// <summary>
        /// Gets the task scheduler.
        /// </summary>
        public TaskScheduler Scheduler { get; }

        /// <inheritdoc />
        public Task<Result> ScheduleExecutionAsync
        (
            Func<IJobContext, CancellationToken, Task> executionTask,
            IJobContext jobContext,
            CancellationToken ct = default
        )
        {
            var wrapped = new Task<Task>(() => executionTask(jobContext, ct));
            wrapped.Start(Scheduler);
            return Task.FromResult(Result.FromSuccess());
        }
    }
}