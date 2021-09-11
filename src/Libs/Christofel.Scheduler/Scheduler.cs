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
        /// <param name="listeners">The listeners that should be fired on events.</param>
        /// <param name="jobThreadScheduler">The scheduler of threads.</param>
        /// <param name="logger">The logger.</param>
        public Scheduler
        (
            IJobStore jobStore,
            IEnumerable<IJobListener> listeners,
            IJobThreadScheduler jobThreadScheduler,
            ILogger<SchedulerThread> logger
        )
        {
            _jobStore = jobStore;
            _schedulerThread = new SchedulerThread
                (this, new SchedulerEventExecutors(listeners), jobStore, jobThreadScheduler, logger);
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
            (IJob job, ITrigger trigger, CancellationToken ct = default) => _jobStore.AddJobAsync(job, trigger);
    }
}