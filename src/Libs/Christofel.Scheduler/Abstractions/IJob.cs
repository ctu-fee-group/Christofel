//
//   IJob.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Remora.Results;

namespace Christofel.Scheduler.Abstractions
{
    /// <summary>
    /// Job that may be executed by <see cref="IScheduler"/>.
    /// </summary>
    public interface IJob
    {
        /// <summary>
        /// Executes the given job.
        /// </summary>
        /// <param name="jobContext">The context of the job.</param>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A result that may not have succeeded.</returns>
        public Task<Result> ExecuteAsync(IJobContext jobContext, CancellationToken ct = default);
    }
}