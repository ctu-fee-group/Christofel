//
//   IJobExecutor.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Remora.Results;

namespace Christofel.Scheduler.Abstractions
{
    /// <summary>
    /// The executor for scheduled jobs that handles all pre and post events and the execution of the job.
    /// </summary>
    public interface IJobExecutor
    {
        /// <summary>
        /// Execute the given job.
        /// </summary>
        /// <remarks>
        /// Should execute <see cref="IJobListener"/>s that are registered,
        /// create the job and execute the job itself.
        /// </remarks>
        /// <param name="jobDescriptor">The descriptor of the job to be executed.</param>
        /// <param name="afterExecutionCallback">The callback to be called after the job was executed.</param>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A result that may not have succeeded.</returns>
        public Task<Result<IJobContext>> BeginExecutionAsync
        (
            IJobDescriptor jobDescriptor,
            Func<IJobDescriptor, Task> afterExecutionCallback,
            CancellationToken ct = default
        );
    }
}