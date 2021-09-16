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
        public bool IsRunning { get; private set; }

        /// <inheritdoc />
        public ValueTask<Result> StartAsync(CancellationToken ct = default)
        {
            IsRunning = true;
            _schedulerThread.Start();
            return ValueTask.FromResult(Result.FromSuccess());
        }

        /// <inheritdoc />
        public ValueTask<Result> StopAsync(CancellationToken ct = default)
        {
            IsRunning = false;
            _schedulerThread.Stop();
            return ValueTask.FromResult(Result.FromSuccess());
        }

        /// <inheritdoc />
        public async ValueTask<Result<IJobDescriptor>> ScheduleAsync
            (IJobData jobData, ITrigger trigger, CancellationToken ct = default)
        {
            var addedResult = await _jobStore.AddJobAsync(jobData, trigger);
            if (addedResult.IsSuccess)
            {
                await _schedulerThread.NotificationBroker.ChangedJobs.NotifyAsync(addedResult.Entity);
            }

            return addedResult;
        }

        /// <inheritdoc />
        public async ValueTask<Result<IJobDescriptor>> RescheduleAsync
            (JobKey jobKey, ITrigger newTrigger, CancellationToken ct = default)
        {
            var jobResult = await _jobStore.GetJobAsync(jobKey);
            if (!jobResult.IsSuccess)
            {
                return jobResult;
            }

            var removedResult = await _jobStore.RemoveJobAsync(jobKey);
            if (!removedResult.IsSuccess)
            {
                return Result<IJobDescriptor>.FromError(removedResult);
            }

            var addedResult = await _jobStore.AddJobAsync(jobResult.Entity.JobData, newTrigger);
            if (addedResult.IsSuccess)
            {
                await _schedulerThread.NotificationBroker.ChangedJobs.NotifyAsync(addedResult.Entity);
            }
            else
            { // Unfortunately we have to report that the job was removed at this point.
                await _schedulerThread.NotificationBroker.RemoveJobs.NotifyAsync(jobKey);
            }

            return addedResult;
        }

        /// <inheritdoc />
        public async ValueTask<Result> UnscheduleAsync(JobKey jobKey, CancellationToken ct = default)
        {
            var removedResult = await _jobStore.RemoveJobAsync(jobKey);
            if (removedResult.IsSuccess)
            {
                await _schedulerThread.NotificationBroker.RemoveJobs.NotifyAsync(jobKey);
            }

            return removedResult;
        }
    }
}