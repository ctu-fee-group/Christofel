//
//   RecurringTrigger.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Scheduler.Abstractions;
using Remora.Results;

namespace Christofel.Scheduler.Triggers
{
    /// <summary>
    /// Triggers the job multiple times after the specified time.
    /// </summary>
    public class RecurringTrigger : ITrigger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringTrigger"/> class.
        /// </summary>
        /// <param name="startDate">The first date of execution.</param>
        /// <param name="recurringInterval">The interval that the job should repeat after.</param>
        public RecurringTrigger(DateTime startDate, TimeSpan recurringInterval)
        {
            NextFireDate = startDate;
            RecurringInterval = recurringInterval;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringTrigger"/> class.
        /// </summary>
        /// <param name="recurringInterval">The interval that the job should repeat after.</param>
        public RecurringTrigger(TimeSpan recurringInterval)
            : this(DateTime.Now, recurringInterval)
        {
        }

        /// <summary>
        /// Gets interval after which the job should be repeated.
        /// </summary>
        public TimeSpan RecurringInterval { get; private set; }

        /// <inheritdoc />
        public ValueTask<bool> CanBeExecutedAsync() => ValueTask.FromResult(true);

        /// <inheritdoc />
        public ValueTask RegisterReadyCallbackAsync(Func<Task> readyTask) => ValueTask.CompletedTask;

        /// <summary>
        /// Gets the date and time of the next execution.
        /// </summary>
        public DateTimeOffset? NextFireDate { get; private set; }

        /// <inheritdoc />
        public ValueTask<Result> BeforeExecutionAsync(IJobContext context, CancellationToken ct = default)
        {
            if (NextFireDate is not null)
            {
                NextFireDate = NextFireDate + RecurringInterval;
            }

            return ValueTask.FromResult(Result.FromSuccess());
        }

        /// <inheritdoc />
        public ValueTask<Result> AfterExecutionAsync
            (IJobContext context, Result jobResult, CancellationToken ct = default)
            => ValueTask.FromResult(Result.FromSuccess());
    }
}