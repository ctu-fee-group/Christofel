//
//   PluginScheduling.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Christofel.Common;
using Christofel.Plugins.Lifetime;
using Christofel.Plugins.Runtime;
using Christofel.Scheduling.Extensions;
using Microsoft.Extensions.Logging;

namespace Christofel.Helpers.Scheduler
{
    /// <summary>
    /// Service for removing plugin jobs from the scheduler job store.
    /// </summary>
    public class PluginScheduling : IStoppable
    {
        private readonly IChristofelState _christofelState;
        private readonly PluginJobsRepository _pluginJobsRepository;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginScheduling"/> class.
        /// </summary>
        /// <param name="christofelState">The state of the application.</param>
        /// <param name="pluginJobsRepository">The plugins job repository.</param>
        /// <param name="logger">The logger.</param>
        public PluginScheduling
        (
            IChristofelState christofelState,
            PluginJobsRepository pluginJobsRepository,
            ILogger<PluginScheduling> logger
        )
        {
            _christofelState = christofelState;
            _pluginJobsRepository = pluginJobsRepository;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken token = default)
        {
            if (_christofelState.Lifetime.State >= LifetimeState.Stopping)
            {
                return;
            }

            var count = 0;

            var jobs = _christofelState.SchedulerJobStore.GetAllJobs();
            var scheduler = _christofelState.Scheduler;
            foreach (var job in jobs)
            {
                if (!_pluginJobsRepository.ContainsType(job.JobData.JobType))
                {
                    continue;
                }

                var unscheduleResult = await scheduler.UnscheduleAsync(job.Key, token);

                if (unscheduleResult.IsSuccess)
                {
                    count++;
                }
                else
                {
                    _logger.LogResult
                    (
                        unscheduleResult,
                        "Could not unschedule plugin's job. The plugin won't be unloaded from the memory."
                    );
                }
            }

            if (count > 0)
            {
                _logger.LogInformation("Removed {Count} scheduler jobs upon stopping the plugin", count);
            }
        }
    }
}