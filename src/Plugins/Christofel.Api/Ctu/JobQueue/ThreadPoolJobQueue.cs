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
    public abstract class ThreadPoolJobQueue<TJob> : IJobQueue<TJob>
    {
        private readonly ILifetime _lifetime;
        private readonly ILogger _logger;
        private readonly Queue<TJob> _queue;

        private bool _threadRunning;

        public ThreadPoolJobQueue(ICurrentPluginLifetime lifetime, ILogger logger)
        {
            _queue = new Queue<TJob>();
            _lifetime = lifetime;
            _logger = logger;
        }

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

        protected abstract Task ProcessAssignJob(TJob job);
    }
}