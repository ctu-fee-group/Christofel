//
//   PluginExecutor.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using Christofel.Scheduler;
using Christofel.Scheduler.Abstractions;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.BaseLib.Implementations.Scheduler
{
    /// <summary>
    /// Job executor for plugins that allows only types that should be handled by the plugin.
    /// </summary>
    public class PluginExecutor : JobExecutor
    {
        private readonly PluginJobsRepository _jobsRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginExecutor"/> class.
        /// </summary>
        /// <param name="threadScheduler">The thread scheduler.</param>
        /// <param name="eventExecutors">The executor of the events.</param>
        /// <param name="services">The services.</param>
        /// <param name="jobsRepository">The repository of the types.</param>
        /// <param name="logger">The logger.</param>
        public PluginExecutor
        (
            IJobThreadScheduler threadScheduler,
            SchedulerEventExecutors eventExecutors,
            IServiceProvider services,
            PluginJobsRepository jobsRepository,
            ILogger<JobExecutor> logger
        )
            : base(threadScheduler, eventExecutors, services, logger)
        {
            _jobsRepository = jobsRepository;
        }

        /// <inheritdoc />
        protected override Result<IJob> CreateJobInstanceAsync
            (IServiceProvider services, IJobDescriptor jobDescriptor, CancellationToken ct = default)
        {
            if (!_jobsRepository.ContainsType(jobDescriptor.JobData.JobType))
            {
                return new InvalidOperationError("This type cannot be handled by this plugin.");
            }

            return base.CreateJobInstanceAsync(services, jobDescriptor, ct);
        }
    }
}