//
//   Scheduler.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Scheduler.Abstractions;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.Scheduler
{
    /// <summary>
    /// Default scheduler scheduling on custom thread.
    /// </summary>
    public class Scheduler : IScheduler
    {
        private readonly SchedulerThread _schedulerThread;
        private readonly IJobStore _jobStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scheduler"/> class.
        /// </summary>
        /// <param name="jobStore">The store of the jobs.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="executor">The executor.</param>
        public Scheduler
        (
            IJobStore jobStore,
            ILogger<SchedulerThread> logger,
            IJobExecutor executor
        )
        {
            _jobStore = jobStore;
            _schedulerThread = new SchedulerThread(jobStore, logger, executor);
        }

        /// <inheritdoc />
        public ValueTask<Result> StartAsync(CancellationToken ct = default)
        {
            _schedulerThread.Start();
            return ValueTask.FromResult(Result.FromSuccess());
        }

        /// <inheritdoc />
        public ValueTask<Result> StopAsync(CancellationToken ct = default)
        {
            _schedulerThread.Stop();
            return ValueTask.FromResult(Result.FromSuccess());
        }

        /// <inheritdoc />
        public ValueTask<Result<IJobDescriptor>> ScheduleAsync
            (IJobData jobData, ITrigger trigger, CancellationToken ct = default) => _jobStore.AddJobAsync
            (jobData, trigger);
    }
}