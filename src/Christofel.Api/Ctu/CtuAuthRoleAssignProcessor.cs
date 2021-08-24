using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Christofel.BaseLib.Lifetime;
using Microsoft.Extensions.Logging;

namespace Christofel.Api.Ctu
{
    /// <summary>
    /// Processor of role assigns, works on different thread
    /// </summary>
    /// <remarks>
    /// Creates thread only if there is job assigned,
    /// if there isn't, the thread is freed (thread pool is used)
    /// </remarks>
    public class CtuAuthRoleAssignProcessor
    {
        private const int MaxRetryCount = 5;

        private record CtuAuthRoleAssign(RestGuildUser User, List<CtuAuthRole> AddRoles, List<CtuAuthRole> RemoveRoles,
            int RetryCount);

        private readonly object _queueLock = new object();
        private readonly Queue<CtuAuthRoleAssign> _queue;

        private bool _threadRunning;

        private ICurrentPluginLifetime _pluginLifetime;

        private readonly ILogger _logger;

        public CtuAuthRoleAssignProcessor(ILogger<CtuAuthRoleAssignProcessor> logger,
            ICurrentPluginLifetime pluginLifetime)
        {
            _logger = logger;
            _queue = new Queue<CtuAuthRoleAssign>();
            _pluginLifetime = pluginLifetime;
        }

        public void EnqueueAssignJob(RestGuildUser user, List<CtuAuthRole> addRoles, List<CtuAuthRole> removeRoles)
        {
            EnqueueAssignJob(new CtuAuthRoleAssign(user, addRoles, removeRoles, 0));
        }

        private void EnqueueAssignJob(CtuAuthRoleAssign assignJob)
        {
            bool createThread = false;
            lock (_queueLock)
            {
                _queue.Enqueue(assignJob);

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

        private void ProcessQueue()
        {
            bool shouldRun = true;
            while (shouldRun)
            {
                try
                {
                    CtuAuthRoleAssign assignJob;

                    lock (_queueLock)
                    {
                        assignJob = _queue.Dequeue();

                        if (_queue.Count == 0)
                        {
                            shouldRun = false;
                            _threadRunning = false;
                        }
                    }

                    ProcessAssignJob(assignJob);
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, "Role assign job has thrown an exception.");
                }
            }

            _logger.LogDebug("Destroying job thread, because no job is queued");
        }

        private void ProcessAssignJob(CtuAuthRoleAssign assignJob)
        {
            try
            {
                assignJob.User.RemoveRolesAsync(assignJob.RemoveRoles.Select(x => x.RoleId).Distinct(),
                    new RequestOptions() { CancelToken = _pluginLifetime.Stopping });
                assignJob.User.AddRolesAsync(assignJob.AddRoles
                        .Select(x => x.RoleId)
                        .Except(assignJob.User.RoleIds)
                        .Distinct(),
                    new RequestOptions() { CancelToken = _pluginLifetime.Stopping });
            }
            catch (Exception e)
            {
                bool retry = assignJob.RetryCount < MaxRetryCount;
                if (retry)
                {
                    EnqueueAssignJob(new CtuAuthRoleAssign(assignJob.User, assignJob.AddRoles, assignJob.RemoveRoles,
                        assignJob.RetryCount + 1));
                }

                _logger.LogError(e,
                    $"Could not assign roles to user {assignJob.User}. Roles to add: {string.Join(",", assignJob.AddRoles.Select(x => x.RoleId))}, Roles to remove: {string.Join(", ", assignJob.RemoveRoles.Select(x => x.RoleId))}. Going to retry: {retry}");
            }
        }
    }
}