//
//   IScheduler.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Remora.Results;

namespace Christofel.Scheduler.Abstractions
{
    /// <summary>
    /// Schedules registered <see cref="IJob"/> by <see cref="ITrigger"/>.
    /// </summary>
    public interface IScheduler
    {
        /// <summary>
        /// Gets whether the work was started.
        /// </summary>
        public bool IsRunning { get; }

        /// <summary>
        /// Starts the process of the scheduling.
        /// </summary>
        /// <remarks>
        /// Starts thread that handles the jobs.
        /// </remarks>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public ValueTask<Result> StartAsync(CancellationToken ct = default);

        /// <summary>
        /// Stops the process of the scheduling.
        /// </summary>
        /// <remarks>
        /// Joins and disposes the thread that handles scheduling.
        /// </remarks>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public ValueTask<Result> StopAsync(CancellationToken ct = default);

        /// <summary>
        /// Schedules the given task that will be executed when <see cref="ITrigger"/> says it should be.
        /// </summary>
        /// <param name="job">The job data that holds information about the job that will be executed.</param>
        /// <param name="trigger">The trigger that schedules the time of the job execution.</param>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public ValueTask<Result<IJobDescriptor>> ScheduleAsync(IJobData job, ITrigger trigger, CancellationToken ct = default);

        /*
        /// <summary>
        /// Reschedules job with the given key to use the new trigger.
        /// </summary>
        /// <remarks>
        /// Joins and disposes the thread that handles scheduling.
        /// </remarks>
        /// <param name="jobKey">The key of the job to reschedule.</param>
        /// <param name="newTrigger">The trigger to save instead of the old one.</param>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public ValueTask<Result<IJobDescriptor>> RescheduleAsync(JobKey jobKey, ITrigger newTrigger, CancellationToken ct = default);

        /// <summary>
        /// Removes the job with the given key so it won't be executed.
        /// </summary>
        /// <param name="jobKey">The key of the job to reschedule.</param>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public ValueTask<Result> UnscheduleAsync(JobKey jobKey, CancellationToken ct = default);
        */
    }
}