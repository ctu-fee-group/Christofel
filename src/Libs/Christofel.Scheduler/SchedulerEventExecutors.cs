//
//   SchedulerEventExecutors.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Scheduler.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Remora.Results;

namespace Christofel.Scheduler
{
    /// <summary>
    /// Service that executes the given <see cref="IJobListener"/>s.
    /// </summary>
    public class SchedulerEventExecutors
    {
        /// <summary>
        /// Executes <see cref="IJobListener.BeforeExecutionAsync"/> events in all of the listeners.
        /// </summary>
        /// <param name="services">The services to create listeners with.</param>
        /// <param name="jobContext">The context of the current job.</param>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A result that may not have succeeded.</returns>
        public async Task<Result> ExecuteBeforeExecutionAsync
            (IServiceProvider services, IJobContext jobContext, CancellationToken ct = default)
        {
            var listeners = services.GetServices<IJobListener>();

            var triggerResult = await jobContext.Trigger.BeforeExecutionAsync(jobContext, ct);
            if (!triggerResult.IsSuccess)
            {
                return triggerResult;
            }

            foreach (var listener in listeners)
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
        /// <param name="services">The services to create listeners with.</param>
        /// <param name="jobContext">The context of the current job.</param>
        /// <param name="jobResult">The result of the job execution.</param>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A result that may not have succeeded.</returns>
        public async Task<Result> ExecuteAfterExecutionAsync
            (IServiceProvider services, IJobContext jobContext, Result jobResult, CancellationToken ct = default)
        {
            var listeners = services.GetServices<IJobListener>();
            var errors = new List<IResult>();

            var triggerResult = await jobContext.Trigger.AfterExecutionAsync(jobContext, jobResult, ct);
            if (!triggerResult.IsSuccess)
            {
                errors.Add(triggerResult);
            }

            foreach (var listener in listeners)
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