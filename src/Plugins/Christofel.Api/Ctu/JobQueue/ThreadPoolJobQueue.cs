//
//   ThreadPoolJobQueue.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Christofel.Plugins.Lifetime;
using Microsoft.Extensions.Logging;

namespace Christofel.Api.Ctu.JobQueue
{
    /// <summary>
    /// Job queue using thread pool queue.
    /// </summary>
    /// <remarks>
    /// Creates thread only if there are any jobs pending,
    /// if there aren't any jobs, no thread will be used.
    /// </remarks>
    /// <typeparam name="TJob">The type of the job.</typeparam>
    public abstract class ThreadPoolJobQueue<TJob> : IJobQueue<TJob>
    {
        private readonly ILifetime _lifetime;
        private readonly ILogger _logger;
        private readonly Queue<TJob> _queue;

        private bool _threadRunning;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadPoolJobQueue{TJob}"/> class.
        /// </summary>
        /// <param name="lifetime">The lifetime of the current plugin.</param>
        /// <param name="logger">The logger.</param>
        public ThreadPoolJobQueue(ICurrentPluginLifetime lifetime, ILogger logger)
        {
            _queue = new Queue<TJob>();
            _lifetime = lifetime;
            _logger = logger;
        }

        /// <inheritdoc />
        public void EnqueueJob(TJob job)
        {
            if (_lifetime.Stopping.IsCancellationRequested)
            {
                _logger.LogWarning("Cannot enqueue more assignment jobs as application is stopping.");
                return;
            }

            var createThread = false;
            lock (_queue)
            {
                _queue.Enqueue(job);

                if (!_threadRunning)
                {
                    createThread = true;
                    _threadRunning = true;
                    Task.Run(ProcessQueue);
                }
            }

            if (createThread)
            {
                _logger.LogDebug("Creating new job thread");
            }
        }

        private async Task ProcessQueue()
        {
            var shouldRun = true;
            while (shouldRun)
            {
                try
                {
                    TJob currentJob;

                    if (_lifetime.Stopping.IsCancellationRequested)
                    {
                        _logger.LogWarning("Could not finish the job as stop was requested");
                        return;
                    }

                    lock (_queue)
                    {
                        currentJob = _queue.Dequeue();
                    }

                    await ProcessAssignJob(currentJob);
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, "Job has thrown an exception.");
                }

                lock (_queue)
                {
                    if (_queue.Count == 0)
                    {
                        shouldRun = false;
                        _threadRunning = false;
                    }
                }
            }

            _logger.LogDebug("Destroying job thread, because no job is queued");
        }

        /// <summary>
        /// Processes given assign job.
        /// </summary>
        /// <param name="job">The job to process.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        protected abstract Task ProcessAssignJob(TJob job);
    }
}