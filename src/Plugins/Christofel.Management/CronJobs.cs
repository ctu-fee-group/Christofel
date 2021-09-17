//
//   CronJobs.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Management.Jobs;
using Christofel.Plugins.Runtime;
using Christofel.Scheduling;
using Christofel.Scheduling;
using Christofel.Scheduling.Triggers;
using Microsoft.Extensions.Logging;

namespace Christofel.Management
{
    /// <summary>
    /// Service for initialization of cron jobs.
    /// </summary>
    public class CronJobs : IStartable
    {
        private readonly IScheduler _scheduler;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CronJobs"/> class.
        /// </summary>
        /// <param name="scheduler">The scheduler.</param>
        /// <param name="logger">The logger.</param>
        public CronJobs(IScheduler scheduler, ILogger<CronJobs> logger)
        {
            _scheduler = scheduler;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken token = default)
        {
            var scheduleResult = await _scheduler.ScheduleAsync
            (
                new TypedJobData<RemoveOldUsersJob>(new JobKey("Cron", "RemoveOldUsers")),
                new RecurringTrigger(TimeSpan.FromDays(1)),
                token
            );

            if (!scheduleResult.IsSuccess)
            {
                _logger.LogError("Could not schedule cron for removing old users {Error}", scheduleResult.Error.Message);
            }
        }
    }
}