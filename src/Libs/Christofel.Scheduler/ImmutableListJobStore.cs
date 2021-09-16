//
//   ImmutableListJobStore.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Christofel.Scheduler.Abstractions;
using Nito.AsyncEx;
using Remora.Results;

namespace Christofel.Scheduler
{
    /// <summary>
    /// Job store using <see cref="ImmutableArray"/>.
    /// </summary>
    public class ImmutableListJobStore : IJobStore
    {
        private readonly AsyncLock _lock;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmutableListJobStore"/> class.
        /// </summary>
        public ImmutableListJobStore()
        {
            _lock = new AsyncLock();
            Data = ImmutableHashSet<IJobDescriptor>.Empty
                .WithComparer(new JobDescriptorEqualityComparer());
        }

        /// <summary>
        /// Gets the data in the store.
        /// </summary>
        public ImmutableHashSet<IJobDescriptor> Data { get; private set; }

        /// <inheritdoc />
        public async ValueTask<Result<IJobDescriptor>> AddJobAsync(IJobData job, ITrigger trigger)
        {
            var jobDescriptor = new JobDescriptor(job, trigger, job.Key);
            if (Data.Contains(jobDescriptor))
            {
                return new InvalidOperationError("There was already item with the same name added.");
            }

            using (await _lock.LockAsync())
            {
                Data = Data.Add(jobDescriptor);
            }

            return jobDescriptor;
        }

        /// <inheritdoc />
        public async ValueTask<Result> RemoveJobAsync(JobKey jobKey)
        {
            using (await _lock.LockAsync())
            {
                Data = Data.Remove(new JobDescriptor(null!, null!, jobKey));
            }

            return Result.FromSuccess();
        }

        /// <inheritdoc />
        public ValueTask<Result<IJobDescriptor>> GetJobAsync(JobKey jobKey)
        {
            if (!Data.TryGetValue(new JobDescriptor(null!, null!, jobKey), out var actualValue))
            {
                return ValueTask.FromResult<Result<IJobDescriptor>>
                    (new NotFoundError("Job with the given name was not found."));
            }

            return ValueTask.FromResult(Result<IJobDescriptor>.FromSuccess(actualValue));
        }

        /// <inheritdoc />
        public ValueTask<IReadOnlyList<IJobDescriptor>> GetJobsTillAsync(DateTimeOffset till)
        {
            return ValueTask.FromResult<IReadOnlyList<IJobDescriptor>>
            (
                Data
                    .Where(x => x.Trigger.NextFireDate is null || x.Trigger.NextFireDate <= till)
                    .OrderBy(x => x.Trigger.NextFireDate ?? DateTimeOffset.UtcNow)
                    .ToList()
            );
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IJobDescriptor> GetAllJobs() => Data;

        private record JobDescriptor(IJobData JobData, ITrigger Trigger, JobKey Key) : IJobDescriptor;
    }
}