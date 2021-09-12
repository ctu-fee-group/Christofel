//
//  IJobRecoverService.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Scheduler.Abstractions;
using Remora.Results;

namespace Christofel.Scheduler.Recoverable
{
    /// <summary>
    /// Service used for recovering job at startup and saving the data.
    /// </summary>
    /// <typeparam name="TJob">The type of the job to be recovered or saved.</typeparam>
    public interface IJobRecoverService<TJob>
        where TJob : IJob
    {
        /// <summary>
        /// Schedules all jobs that are to be recovered.
        /// </summary>
        /// <param name="scheduler">The scheduler.</param>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A result that may not have succeeded.</returns>
        public Task<Result<IReadOnlyList<IJobData>>> RecoverJobsAsync(IScheduler scheduler, CancellationToken ct = default);

        /// <summary>
        /// Saves job data to the persistent store.
        /// </summary>
        /// <param name="job">The job to be saved.</param>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A result that may not have succeeded.</returns>
        public Task<Result> SaveJobDataAsync(IJobData job, CancellationToken ct = default);

        /// <summary>
        /// Removes job data from the persistent store.
        /// </summary>
        /// <param name="job">The job data to be removed.</param>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A result that may not have succeeded.</returns>
        public Task<Result> RemoveJobDataAsync(IJobData job, CancellationToken ct = default);

        /// <summary>
        /// Saves job data to the persistent store and then schedules it to scheduler.
        /// </summary>
        /// <param name="scheduler">The scheduler to schedule the job with.</param>
        /// <param name="job">The job to be saved.</param>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A result that may not have succeeded.</returns>
        public Task<Result<IJobDescriptor>> SaveAndScheduleJobAsync(IScheduler scheduler, IJobData job, CancellationToken ct = default);
    }
}