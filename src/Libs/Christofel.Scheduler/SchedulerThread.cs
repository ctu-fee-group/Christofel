//
//   SchedulerThread.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Scheduler.Abstractions;
using Christofel.Scheduler.Extensions;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using Remora.Results;

namespace Christofel.Scheduler
{
    /// <summary>
    /// Executes jobs from the storage.
    /// </summary>
    public class SchedulerThread
    {
        private static readonly TimeSpan _getJobsTillTimespan = TimeSpan.FromMinutes(30);

        private readonly IJobStore _jobStore;
        private readonly ILogger _logger;
        private readonly IJobExecutor _executor;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly AsyncAutoResetEvent _workResetEvent;

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
            _cancellationTokenSource = new CancellationTokenSource();

            _workResetEvent = new AsyncAutoResetEvent();
            NotificationBroker = new SchedulerThreadNotificationBroker(_workResetEvent);
        }

        /// <summary>
        /// Gets the broker of notifications.
        /// </summary>
        public SchedulerThreadNotificationBroker NotificationBroker { get; }

        /// <summary>
        /// Starts the scheduler thread.
        /// </summary>
        public void Start()
        {
            Task.Run(
                async () =>
                {
                    try
                    {
                        await Run();
                    }
                    catch (Exception e)
                    {
                        _logger.LogCritical(e, "There was an exception inside of SchedulerThread. The Scheduler won't work correctly.");
                    }
                });
        }

        /// <summary>
        /// Exists the scheduler thread.
        /// </summary>
        public void Stop()
        {
            _workResetEvent.Set();
            _cancellationTokenSource.Cancel();
        }

        private async Task Run()
        {
            HashSet<IJobDescriptor> executingJobs = new HashSet<IJobDescriptor>(new JobDescriptorEqualityComparer());
            AsyncLock executingJobsLock = new AsyncLock();
            var shouldWaitNext = false;

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var list = await _jobStore.GetJobsTillAsync(DateTimeOffset.UtcNow.Add(_getJobsTillTimespan));
                var queue = new PriorityQueue<IJobDescriptor, DateTimeOffset>();
                for (var i = 0; i < list.Count; i++)
                {
                    queue.Enqueue(list[i], list[i].Trigger.NextFireDate ?? DateTimeOffset.UtcNow);
                }

                list = null;

                await CheckNotificationsAsync(queue);

                if (queue.Count == 0 || shouldWaitNext)
                {
                    shouldWaitNext = false;
                    using var delayedCancellationToken = new CancellationTokenSource();
                    delayedCancellationToken.CancelAfter(_getJobsTillTimespan);
                    await _workResetEvent.WaitSafeAsync(delayedCancellationToken.Token);

                    continue;
                }

                shouldWaitNext = true;

                // ReSharper disable once ForCanBeConvertedToForeach
                while (queue.Count > 0)
                {
                    try
                    {
                        var dequeued = queue.TryDequeue(out var job, out _);
                        if (!dequeued)
                        {
                            break;
                        }

                        using (await executingJobsLock.LockAsync())
                        {
                            if (executingJobs.Contains(job!))
                            {
                                continue;
                            }
                        }

                        var fireTime = job!.Trigger.NextFireDate;
                        if (fireTime is not null && fireTime > DateTimeOffset.UtcNow)
                        {
                            using var delayedCancellationToken = new CancellationTokenSource();
                            delayedCancellationToken.CancelAfter((DateTimeOffset)fireTime - DateTimeOffset.UtcNow);
                            await _workResetEvent.WaitSafeAsync(delayedCancellationToken.Token);
                        }
                        else if (fireTime is null)
                        {
                            shouldWaitNext = false;
                            var removeResult = await _jobStore.RemoveJobAsync(job.Key);
                            if (!removeResult.IsSuccess)
                            {
                                _logger.LogError("Could not remove job {Job} from the store", job.Key);
                            }

                            continue;
                        }

                        if (fireTime > DateTimeOffset.UtcNow)
                        {
                            shouldWaitNext = false;
                            queue.Enqueue(job, (DateTimeOffset)fireTime);
                            await CheckNotificationsAsync(queue);
                        }
                        else if (!await job.Trigger.CanBeExecutedAsync())
                        {
                            using (await executingJobsLock.LockAsync())
                            {
                                executingJobs.Add(job);
                            }

                            await job.Trigger.RegisterReadyCallbackAsync
                            (
                                async () =>
                                {
                                    using (await executingJobsLock.LockAsync())
                                    {
                                        executingJobs.Remove(job);
                                    }

                                    // TODO decide: should this be cancellable by removing the job?
                                    await NotificationBroker.ExecuteJobs.NotifyAsync(job);
                                }
                            );
                        }
                        else
                        {
                            shouldWaitNext = false;
                            var beginExecutionResult = await _executor.BeginExecutionAsync
                            (
                                job,
                                async (job) =>
                                {
                                    using (await executingJobsLock.LockAsync())
                                    {
                                        executingJobs.Remove(job);
                                    }

                                    await NotificationBroker.ChangedJobs.NotifyAsync(job);
                                },
                                _cancellationTokenSource.Token
                            );

                            if (!beginExecutionResult.IsSuccess)
                            {
                                // TODO figure out how to handle this state.
                                _logger.LogError("Could not begin execution of {Job}", job.Key);
                            }
                            else
                            {
                                using (await executingJobsLock.LockAsync())
                                {
                                    executingJobs.Add(job);
                                }
                            }

                            if (job.Trigger.NextFireDate is null)
                            {
                                var removeResult = await _jobStore.RemoveJobAsync(job.Key);
                                if (!removeResult.IsSuccess)
                                {
                                    _logger.LogError("Could not remove job {Job} from the store", job.Key);
                                }
                            }
                        }
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
            }
        }

        private async Task<PriorityQueue<IJobDescriptor, DateTimeOffset>> CheckNotificationsAsync
            (PriorityQueue<IJobDescriptor, DateTimeOffset> enqueuedJobs)
        {
            var changeJobs = new Dictionary<JobKey, IJobDescriptor?>();

            if (await NotificationBroker.ExecuteJobs.HasPendingNotifications())
            {
                (IDisposable @lock, Queue<IJobDescriptor> jobs) =
                    await NotificationBroker.ExecuteJobs.GetNotifications();
                using (@lock)
                {
                    while (jobs.TryDequeue(out var job))
                    {
                        enqueuedJobs.Enqueue(job, job.Trigger.NextFireDate ?? DateTimeOffset.UtcNow);
                    }
                }
            }

            if (await NotificationBroker.ChangedJobs.HasPendingNotifications())
            {
                (IDisposable @lock, Queue<IJobDescriptor> jobs) =
                    await NotificationBroker.ChangedJobs.GetNotifications();
                using (@lock)
                {
                    while (jobs.TryDequeue(out var job))
                    {
                        IJobDescriptor? storedJob = null;
                        foreach (var x in enqueuedJobs.UnorderedItems)
                        {
                            var x1 = x.Element;
                            if (x1.Key == job.Key)
                            {
                                storedJob = x1;
                                break;
                            }
                        }

                        if (storedJob is null)
                        {
                            enqueuedJobs.Enqueue(job, job.Trigger.NextFireDate ?? DateTimeOffset.UnixEpoch);
                        }
                        else if (storedJob != job)
                        {
                            changeJobs[job.Key] = job;
                        }
                    }
                }
            }

            if (await NotificationBroker.RemoveJobs.HasPendingNotifications())
            {
                (IDisposable @lock, Queue<JobKey> jobs) = await NotificationBroker.RemoveJobs.GetNotifications();
                using (@lock)
                {
                    while (jobs.TryDequeue(out var jobKey))
                    {
                        var containsJob = false;
                        foreach (var x in enqueuedJobs.UnorderedItems)
                        {
                            if (x.Element.Key == jobKey)
                            {
                                containsJob = true;
                                break;
                            }
                        }

                        if (containsJob)
                        {
                            changeJobs[jobKey] = null;
                        }
                    }
                }
            }

            if (changeJobs.Count != 0)
            {
                var storedDequeuedList = new List<IJobDescriptor>();
                while (changeJobs.Count > 0)
                {
                    if (enqueuedJobs.TryDequeue(out var storedJob, out _))
                    {
                        if (changeJobs.ContainsKey(storedJob.Key))
                        {
                            var swapJob = changeJobs[storedJob.Key];
                            if (swapJob is not null)
                            {
                                enqueuedJobs.Enqueue(swapJob, swapJob.Trigger.NextFireDate ?? DateTimeOffset.UnixEpoch);
                            }

                            changeJobs.Remove(storedJob.Key);
                        }
                        else
                        {
                            storedDequeuedList.Add(storedJob);
                        }
                    }
                }

                foreach (var storedDequeued in storedDequeuedList)
                {
                    enqueuedJobs.Enqueue(storedDequeued, storedDequeued.Trigger.NextFireDate ?? DateTimeOffset.UnixEpoch);
                }
                storedDequeuedList.Clear();
            }

            return enqueuedJobs;
        }
    }
}