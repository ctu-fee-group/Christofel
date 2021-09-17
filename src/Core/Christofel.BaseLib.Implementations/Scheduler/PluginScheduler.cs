//
//   PluginScheduler.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Christofel.Scheduling;
using Remora.Results;

namespace Christofel.BaseLib.Implementations.Scheduler
{
    /// <summary>
    /// Scheduler without scheduling thread.
    /// </summary>
    public class PluginScheduler : IScheduler
    {
        private readonly IJobStore _jobStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginScheduler"/> class.
        /// </summary>
        /// <param name="jobStore">The storage for the jobs.</param>
        public PluginScheduler(IJobStore jobStore)
        {
            _jobStore = jobStore;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>Not supported error.</returns>
        public ValueTask<Result> StartAsync(CancellationToken ct = default) => ValueTask.FromResult<Result>
            (new NotSupportedError());

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>Not supported error.</returns>
        public ValueTask<Result> StopAsync(CancellationToken ct = default) => ValueTask.FromResult<Result>
            (new NotSupportedError());

        /// <inheritdoc />
        public ValueTask<Result<IJobDescriptor>> ScheduleAsync
            (IJobData job, ITrigger trigger, CancellationToken ct = default)
            => _jobStore.AddJobAsync(job, trigger);
    }
}