//
//   EntityJobRecoverableService.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Scheduler.Abstractions;
using Remora.Results;

namespace Christofel.Scheduler.Recoverable
{
    /// <summary>
    /// Recoverable service that supports job with specified entity data.
    /// </summary>
    /// <typeparam name="TJob">The type of the job.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public abstract class EntityJobRecoverableService<TJob, TEntity> : IJobRecoverService<TJob>
        where TJob : IDataJob<TEntity>
    {
        /// <inheritdoc />
        public async Task<Result<IReadOnlyList<TJob>>> RecoverJobsAsync
            (IScheduler scheduler, CancellationToken ct = default)
        {
            var entitiesResult = await GetEntitiesAsync(ct);
            if (!entitiesResult.IsSuccess)
            {
                return Result<IReadOnlyList<TJob>>.FromError(entitiesResult);
            }

            var jobs = new List<TJob>();
            var errors = new List<IResult>();
            foreach (var entity in entitiesResult.Entity)
            {
                var job = CreateJob(entity);
                var trigger = CreateTrigger(job);
                var scheduleResult = await ScheduleJobAsync(scheduler, job, trigger, ct);
                if (!scheduleResult.IsSuccess)
                {
                    errors.Add(scheduleResult);
                }
                else
                {
                    jobs.Add(job);
                }
            }

            return errors.Count > 0
                ? new AggregateError(errors)
                : Result<IReadOnlyList<TJob>>.FromSuccess(jobs);
        }

        /// <inheritdoc />
        public Task<Result> SaveJobDataAsync
            (TJob job, CancellationToken ct = default)
            => SaveEntityAsync(job.Data, ct);

        /// <inheritdoc />
        public Task<Result> RemoveJobDataAsync(TJob job, CancellationToken ct = default) => RemoveEntityAsync
            (job.Data, ct);

        /// <inheritdoc />
        public async Task<Result<IJobDescriptor>> SaveAndScheduleJobAsync
            (IScheduler scheduler, TJob job, CancellationToken ct = default)
        {
            var saveResult = await SaveJobDataAsync(job, ct);
            if (!saveResult.IsSuccess)
            {
                return Result<IJobDescriptor>.FromError(saveResult);
            }

            return await ScheduleJobAsync(scheduler, job, CreateTrigger(job), ct);
        }

        /// <summary>
        /// Schedules the given job.
        /// </summary>
        /// <param name="scheduler">The scheduler to schedule the job with.</param>
        /// <param name="job">The job to be scheduled.</param>
        /// <param name="trigger">The trigger to be used.</param>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A result that may not have succeeded.</returns>
        protected virtual async Task<Result<IJobDescriptor>> ScheduleJobAsync
            (IScheduler scheduler, TJob job, ITrigger trigger, CancellationToken ct = default)
        {
            var scheduleResult = await scheduler.ScheduleAsync(job, trigger, ct);
            if (!scheduleResult.IsSuccess)
            {
                return scheduleResult;
            }

            await OnJobScheduled(scheduleResult.Entity);
            return scheduleResult;
        }

        /// <summary>
        /// Gets entities that should be recovered.
        /// </summary>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A result that may not have succeeded.</returns>
        protected abstract Task<Result<IReadOnlyList<TEntity>>> GetEntitiesAsync(CancellationToken ct = default);

        /// <summary>
        /// Saves the entity to persistent store.
        /// </summary>
        /// <param name="entity">The entity to be saved.</param>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A result that may not have succeeded.</returns>
        protected abstract Task<Result> SaveEntityAsync(TEntity entity, CancellationToken ct = default);

        /// <summary>
        /// Removes the entity from persistent store.
        /// </summary>
        /// <param name="entity">The entity to be removed.</param>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A result that may not have succeeded.</returns>
        protected abstract Task<Result> RemoveEntityAsync(TEntity entity, CancellationToken ct = default);

        /// <summary>
        /// Creates instance of job with the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be passed to the job.</param>
        /// <returns>Created job with the given entity.</returns>
        protected abstract TJob CreateJob(TEntity entity);

        /// <summary>
        /// Creates instance of trigger for the specified job.
        /// </summary>
        /// <param name="job">The job to be scheduled.</param>
        /// <returns>Created job with the given entity.</returns>
        protected abstract ITrigger CreateTrigger(TJob job);

        /// <summary>
        /// Executes after job is scheduled using the scheduler.
        /// </summary>
        /// <param name="job">The descriptor for the job.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        protected virtual Task OnJobScheduled(IJobDescriptor job) => Task.CompletedTask;
    }
}