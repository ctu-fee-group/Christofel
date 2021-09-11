//
//   ApplicationScheduler.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Plugins.Runtime;
using Christofel.Scheduler;
using Christofel.Scheduler.Abstractions;
using Microsoft.Extensions.Logging;

namespace Christofel.Application.Scheduler
{
    /// <inheritdoc cref="Christofel.Scheduler.Scheduler" />
    public class ApplicationScheduler : Christofel.Scheduler.Scheduler, IStartable, IStoppable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationScheduler"/> class.
        /// </summary>
        /// <param name="jobStore">The store of the jobs.</param>
        /// <param name="listeners">The listeners that should be fired on events.</param>
        /// <param name="jobThreadScheduler">The scheduler of threads.</param>
        /// <param name="logger">The logger.</param>
        public ApplicationScheduler
        (
            IJobStore jobStore,
            IEnumerable<IJobListener> listeners,
            IJobThreadScheduler jobThreadScheduler,
            ILogger<SchedulerThread> logger
        )
            : base(jobStore, listeners, jobThreadScheduler, logger)
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