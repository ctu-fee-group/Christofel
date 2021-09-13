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
using Christofel.Scheduler.Extensions;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.Scheduler
{
    /// <summary>
    /// Executes jobs from the storage.
    /// </summary>
    public class SchedulerThread
    {
        private readonly IJobStore _jobStore;
        private readonly ILogger _logger;
        private readonly IJobExecutor _executor;
        private readonly HashSet<IJobDescriptor> _executingJobs;
        private readonly CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerThread"/> class.
        /// </summary>
        /// <param name="jobStore">The storage for the jobs.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="executor">The executor that executes the given jobs.</param>
        public SchedulerThread
        (
            IJobStore jobStore,
            ILogger<SchedulerThread> logger,
            IJobExecutor executor
        )
        {
            _jobStore = jobStore;
            _logger = logger;
            _executor = executor;
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
                var list = _jobStore.EnumerateJobs();

                // ReSharper disable once ForCanBeConvertedToForeach
                for (int i = 0; i < list.Count; i++)
                {
                    try
                    {
                        var job = list[i];
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
                            lock (_executingJobs)
                            {
                                _executingJobs.Add(job);
                            }

                            var beginExecutionResult = await _executor.BeginExecutionAsync
                            (
                                job,
                                returnedJob =>
                                {
                                    lock (_executingJobs)
                                    {
                                        _executingJobs.Remove(returnedJob);
                                    }

                                    return Task.CompletedTask;
                                },
                                _cancellationTokenSource.Token
                            );

                            if (!beginExecutionResult.IsSuccess)
                            {
                                _logger.LogResult(beginExecutionResult, $"Could not begin execution of job {job.Key}");
                            }
                        }

                        if (job.Trigger.CanBeDeleted())
                        {
                            var removalResult = await _jobStore.RemoveJobAsync(job.Key);
                            if (!removalResult.IsSuccess)
                            {
                                _logger.LogResult
                                    (removalResult, $"Could not remove job description of {job.Key} from the store");
                            }
                        }

                        await Task.Delay(100);
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
    }
}