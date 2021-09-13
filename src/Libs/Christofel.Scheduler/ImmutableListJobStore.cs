//
//   ImmutableListJobStore.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Christofel.Scheduler.Abstractions;
using Remora.Results;

namespace Christofel.Scheduler
{
    /// <summary>
    /// Job store using <see cref="ImmutableArray"/>.
    /// </summary>
    public class ImmutableListJobStore : IJobStore
    {
        private readonly object _lock;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmutableListJobStore"/> class.
        /// </summary>
        public ImmutableListJobStore()
        {
            _lock = new object();
            Data = ImmutableList<IJobDescriptor>.Empty;
        }

        /// <summary>
        /// Gets the data in the store.
        /// </summary>
        public ImmutableList<IJobDescriptor> Data { get; private set; }

        /// <inheritdoc />
        public ValueTask<Result<IJobDescriptor>> AddJobAsync(IJobData job, ITrigger trigger)
        {
            var jobDescriptor = new JobDescriptor(job, trigger, job.Key);
            lock (_lock)
            {
                Data = Data.Add(jobDescriptor);
            }

            return ValueTask.FromResult<Result<IJobDescriptor>>(jobDescriptor);
        }

        /// <inheritdoc />
        public ValueTask<Result> RemoveJobAsync(JobKey jobKey)
        {
            lock (_lock)
            {
                foreach (var job in Data)
                {
                    if (job.Key == jobKey)
                    {
                        Data = Data.Remove(job);
                    }
                }
            }

            return ValueTask.FromResult<Result>(Result.FromSuccess());
        }

        /// <inheritdoc />
        public IReadOnlyList<IJobDescriptor> EnumerateJobs() => Data;

        private record JobDescriptor(IJobData JobData, ITrigger Trigger, JobKey Key) : IJobDescriptor;
    }
}