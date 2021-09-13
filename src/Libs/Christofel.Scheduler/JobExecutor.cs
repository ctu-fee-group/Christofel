//
//   JobExecutor.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Scheduler.Abstractions;
using Christofel.Scheduler.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.Scheduler
{
    /// <summary>
    /// Default executor for jobs that uses <see cref="IServiceProvider"/>.
    /// </summary>
    public class JobExecutor : IJobExecutor
    {
        private readonly IJobThreadScheduler _threadScheduler;
        private readonly SchedulerEventExecutors _eventExecutors;
        private readonly IServiceProvider _services;
        private readonly ILogger<JobExecutor> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobExecutor"/> class.
        /// </summary>
        /// <param name="threadScheduler">The thread scheduler.</param>
        /// <param name="eventExecutors">The executor of the events.</param>
        /// <param name="services">The services.</param>
        /// <param name="logger">The logger.</param>
        public JobExecutor
        (
            IJobThreadScheduler threadScheduler,
            SchedulerEventExecutors eventExecutors,
            IServiceProvider services,
            ILogger<JobExecutor> logger
        )
        {
            _threadScheduler = threadScheduler;
            _eventExecutors = eventExecutors;
            _services = services;
            _logger = logger;
        }

        /// <inheritdoc />
        public virtual async Task<Result<IJobContext>> BeginExecutionAsync
            (IJobDescriptor jobDescriptor, Func<IJobDescriptor, Task> afterExecutionCallback, CancellationToken ct)
        {
            var scope = _services.CreateScope();
            var services = scope.ServiceProvider;

            var createdJobResult = CreateJobInstanceAsync(services, jobDescriptor, ct);
            if (!createdJobResult.IsSuccess)
            {
                scope.Dispose();
                return Result<IJobContext>.FromError(createdJobResult);
            }

            var jobContext = new JobContext(jobDescriptor, createdJobResult.Entity, jobDescriptor.Trigger);
            var beforeEventResult = await _eventExecutors.ExecuteBeforeExecutionAsync(services, jobContext, ct);
            if (!beforeEventResult.IsSuccess)
            {
                _logger.LogResult(beforeEventResult, "Before execution events failed");
            }

            var result = await _threadScheduler.ScheduleExecutionAsync
                ((job, token) => WrappedJobExecutor(scope, job, afterExecutionCallback, token), jobContext, ct);
            if (!result.IsSuccess)
            {
                _logger.LogResult(result, "Could not schedule execution {Error}");
            }

            return Result<IJobContext>.FromSuccess(jobContext);
        }

        /// <summary>
        /// Creates instance of the given job.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="jobDescriptor">The descriptor of the job to be created.</param>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A job that instance represents the given descriptor.</returns>
        protected virtual Result<IJob> CreateJobInstanceAsync
            (IServiceProvider services, IJobDescriptor jobDescriptor, CancellationToken ct = default)
        {
            var data = jobDescriptor.JobData;
            if (data.JobInstance is not null)
            {
                return Result<IJob>.FromSuccess(data.JobInstance);
            }

            if (!data.JobType.IsAssignableTo(typeof(IJob)))
            {
                return new InvalidOperationError($"The given type {data.JobType} is not assignable to IJob.");
            }

            IJob createdInstance;
            try
            {
                createdInstance = (IJob)ActivatorUtilities.CreateInstance(services, data.JobType, data.Data.Values.ToArray());
            }
            catch (Exception e)
            {
                return e;
            }

            return Result<IJob>.FromSuccess(createdInstance);
        }

        private async Task WrappedJobExecutor
        (
            IServiceScope scope,
            IJobContext jobContext,
            Func<IJobDescriptor, Task> afterExecutionCallback,
            CancellationToken ct
        )
        {
            Func<Task<Result>> wrapped = async () =>
            {
                try
                {
                    return await jobContext.Job.ExecuteAsync(jobContext, ct);
                }
                catch (Exception e)
                {
                    return e;
                }
            };

            var executionResult = await wrapped();

            var afterEventResult = await _eventExecutors.ExecuteAfterExecutionAsync
                (scope.ServiceProvider, jobContext, executionResult, ct);
            if (!afterEventResult.IsSuccess)
            {
                _logger.LogResult(afterEventResult, "After execution events failed");
            }

            scope.Dispose();

            await afterExecutionCallback(jobContext.JobDescriptor);
        }

        private record JobContext(IJobDescriptor JobDescriptor, IJob Job, ITrigger Trigger) : IJobContext;
    }
}