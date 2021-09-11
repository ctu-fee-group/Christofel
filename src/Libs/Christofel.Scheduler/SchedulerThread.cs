//
//   SchedulerThread.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Scheduler.Abstractions;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.Scheduler
{
    /// <summary>
    /// Executes jobs from the storage.
    /// </summary>
    public class SchedulerThread
    {
        private readonly SchedulerEventExecutors _eventExecutors;
        private readonly IJobStore _jobStore;
        private readonly IJobThreadScheduler _jobThreadScheduler;
        private readonly ILogger _logger;
        private readonly HashSet<IJobDescriptor> _executingJobs;
        private readonly CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerThread"/> class.
        /// </summary>
        /// <param name="eventExecutors">The executor of listener events.</param>
        /// <param name="jobStore">The storage for the jobs.</param>
        /// <param name="jobThreadScheduler">The job thread scheduler.</param>
        /// <param name="logger">The logger.</param>
        public SchedulerThread
        (
            SchedulerEventExecutors eventExecutors,
            IJobStore jobStore,
            IJobThreadScheduler jobThreadScheduler,
            ILogger<SchedulerThread> logger
        )
        {
            _eventExecutors = eventExecutors;
            _jobStore = jobStore;
            _jobThreadScheduler = jobThreadScheduler;
            _logger = logger;
            _executingJobs = new HashSet<IJobDescriptor>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Starts the scheduler thread.
        /// </summary>
        public void Start()
        {
            Task.Run(Run);
        }

        /// <summary>
        /// Exists the scheduler thread.
        /// </summary>
        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }

        private async Task Run()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                bool jobsEnqueued = false;
                foreach (var job in _jobStore.EnumerateJobs())
                {
                    try
                    {
                        jobsEnqueued = true;

                        lock (_executingJobs)
                        {
                            if (_executingJobs.Contains(job))
                            {
                                continue;
                            }
                        }

                        if (job.Trigger.ShouldBeExecuted())
                        {
                            await ExecuteJob(job, _cancellationTokenSource.Token);
                        }

                        if (job.Trigger.CanBeDeleted())
                        {
                            var removalResult = await _jobStore.RemoveJobAsync(job.Key);
                            if (!removalResult.IsSuccess)
                            {
                                LogResult("Could not remove trigger from the store", removalResult);
                            }
                        }

                        await Task.Delay(1);
                    }
                    catch (OperationCanceledException)
                    {
                        // ignored
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "An exception was thrown in the scheduler thread");
                    }
                }

                if (!jobsEnqueued)
                {
                    await Task.Delay(1000);
                }
            }
        }

        private async Task ExecuteJob(IJobDescriptor job, CancellationToken ct)
        {
            lock (_executingJobs)
            {
                _executingJobs.Add(job);
            }

            var jobContext = new JobContext(job, job.Job, job.Trigger);
            var beforeEventResult = await _eventExecutors.ExecuteBeforeExecutionAsync(jobContext, ct);
            if (!beforeEventResult.IsSuccess)
            {
                LogResult("Before execution events failed", beforeEventResult);
            }

            var result = await _jobThreadScheduler.ScheduleExecutionAsync
                (WrappedJobExecutor, jobContext, _cancellationTokenSource.Token);
            if (!result.IsSuccess)
            {
                LogResult("Could not schedule execution {Error}", result);
            }
        }

        private async Task WrappedJobExecutor(IJobContext jobContext, CancellationToken ct)
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

            var afterEventResult = await _eventExecutors.ExecuteAfterExecutionAsync(jobContext, executionResult, ct);
            if (!afterEventResult.IsSuccess)
            {
                LogResult("After execution events failed", afterEventResult);
            }

            if (jobContext.Trigger.CanBeDeleted())
            {
                var removalResult = await _jobStore.RemoveJobAsync(jobContext.JobDescriptor.Key);
                if (!removalResult.IsSuccess)
                {
                    LogResult("Could not remove trigger from the store", removalResult);
                }
            }

            lock (_executingJobs)
            {
                _executingJobs.Remove(jobContext.JobDescriptor);
            }
        }

        private void LogResult(string message, IResult result)
        {
            if (result.IsSuccess)
            {
                return;
            }

            switch (result.Error)
            {
                case ExceptionError exeptionError:
                    _logger.LogError(exeptionError.Exception, message);
                    break;
                case AggregateError aggregateError:
                    foreach (var errorResult in aggregateError.Errors)
                    {
                        LogResult(message, errorResult);
                    }
                    break;
                default:
                    _logger.LogError("{Message} {Error}", message, result.Error?.Message);
                    break;
            }
        }

        private record JobContext(IJobDescriptor JobDescriptor, IJob Job, ITrigger Trigger) : IJobContext;
    }
}