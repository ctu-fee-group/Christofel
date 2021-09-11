//
//   SchedulerEventExecutors.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Scheduler.Abstractions;
using Remora.Results;

namespace Christofel.Scheduler
{
    /// <summary>
    /// Service that executes the given <see cref="IJobListener"/>s.
    /// </summary>
    public class SchedulerEventExecutors
    {
        private readonly IReadOnlyList<IJobListener> _listeners;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerEventExecutors"/> class.
        /// </summary>
        /// <param name="listeners">The listeners to call.</param>
        public SchedulerEventExecutors(IEnumerable<IJobListener> listeners)
        {
            _listeners = listeners.ToList();
        }

        /// <summary>
        /// Executes <see cref="IJobListener.BeforeExecutionAsync"/> events in all of the listeners.
        /// </summary>
        /// <param name="jobContext">The context of the current job.</param>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A result that may not have succeeded.</returns>
        public async Task<Result> ExecuteBeforeExecutionAsync(IJobContext jobContext, CancellationToken ct = default)
        {
            var triggerResult = await jobContext.Trigger.BeforeExecutionAsync(jobContext, ct);
            if (!triggerResult.IsSuccess)
            {
                return triggerResult;
            }

            foreach (var listener in _listeners)
            {
                var result = await listener.BeforeExecutionAsync(jobContext, ct);
                if (!result.IsSuccess)
                {
                    return result;
                }
            }

            return Result.FromSuccess();
        }

        /// <summary>
        /// Executes <see cref="IJobListener.AfterExecutionAsync"/> events in all of the listeners.
        /// </summary>
        /// <param name="jobContext">The context of the current job.</param>
        /// <param name="jobResult">The result of the job execution.</param>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A result that may not have succeeded.</returns>
        public async Task<Result> ExecuteAfterExecutionAsync
            (IJobContext jobContext, Result jobResult, CancellationToken ct = default)
        {
            var errors = new List<IResult>();

            var triggerResult = await jobContext.Trigger.AfterExecutionAsync(jobContext, jobResult, ct);
            if (!triggerResult.IsSuccess)
            {
                errors.Add(triggerResult);
            }

            foreach (var listener in _listeners)
            {
                var result = await listener.AfterExecutionAsync(jobContext, jobResult, ct);
                if (!result.IsSuccess)
                {
                    errors.Add(result);
                }
            }

            return errors.Count > 0
                ? new AggregateError(errors)
                : Result.FromSuccess();
        }
    }
}