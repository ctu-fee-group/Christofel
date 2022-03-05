//
//   RecoverableRetryProvider.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Christofel.Scheduler.Abstractions;
using Christofel.Scheduler.Retryable;
using Remora.Results;

namespace Christofel.Scheduler.Recoverable
{
    /*
    public abstract class RecoverableRetryProvider : IRetryProvider
    {
        public RecoverableRetryProvider(){


        /// <inheritdoc />
        public int MaxRepeatCount { get; set; }

        /// <inheritdoc />
        public Task<Result> ScheduleRepeatAsync
            (IJobContext jobContext, Result jobResult, CancellationToken ct = default)
        {

        }

        /// <summary>
        /// Creates <see cref="ITrigger"/> for the job.
        /// </summary>
        /// <param name="context">The context to create trigger for.</param>
        /// <returns>A trigger.</returns>
        protected abstract ITrigger CreateRetryTrigger(IJobContext context);
    }
    */
}