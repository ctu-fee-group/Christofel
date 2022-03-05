//
//   ApplicationScheduler.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Plugins.Runtime;
using Christofel.Scheduling;
using Microsoft.Extensions.Logging;

namespace Christofel.Application.Scheduler
{
    /// <inheritdoc cref="Scheduling.Scheduler" />
    public class ApplicationScheduler : Scheduling.Scheduler, IStartable, IStoppable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationScheduler"/> class.
        /// </summary>
        /// <param name="jobStore">The store of the jobs.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="executor">The executor.</param>
        public ApplicationScheduler
        (
            IJobStore jobStore,
            ILogger<SchedulerThread> logger,
            IJobExecutor executor
        )
            : base(jobStore, logger, executor)
        {
        }

        /// <inheritdoc />
        async Task IStartable.StartAsync(CancellationToken token)
        {
            await StartAsync(token);
        }

        /// <inheritdoc />
        async Task IStoppable.StopAsync(CancellationToken token)
        {
            await StopAsync(token);
        }
    }
}