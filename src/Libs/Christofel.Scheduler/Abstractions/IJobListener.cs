//
//   IJobListener.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Remora.Results;

namespace Christofel.Scheduler.Abstractions
{
    /// <summary>
    /// Listener to <see cref="IJob"/> execution events that are fired by <see cref="IScheduler"/>.
    /// </summary>
    public interface IJobListener
    {
        /// <summary>
        /// Processes event before execution of the given job.
        /// </summary>
        /// <remarks>
        /// Non successful events will ensure the job is not executed.
        /// </remarks>
        /// <param name="context">The context of the current job.</param>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A result that may not have been successful.</returns>
        public ValueTask<Result> BeforeExecutionAsync
            (IJobContext context, CancellationToken ct = default);

        /// <summary>
        /// Processes event after execution of the given job.
        /// </summary>
        /// <param name="context">The context of the current job.</param>
        /// <param name="jobResult">The result of the job that may not have succeeded.</param>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A result that may not have been successful.</returns>
        public ValueTask<Result> AfterExecutionAsync
            (IJobContext context, Result jobResult, CancellationToken ct = default);
    }
}