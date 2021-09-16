//
//   DelayedTrigger.cs
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
    /// Trigger that delays the job for the specified time.
    /// </summary>
    public class DelayedTrigger : ITrigger
    {
        private bool _executed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelayedTrigger"/> class.
        /// </summary>
        /// <param name="duration">The duration after which the trigger should be executed.</param>
        public DelayedTrigger(TimeSpan duration)
        {
            NextFireDate = DateTimeOffset.Now.Add(duration);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelayedTrigger"/> class.
        /// </summary>
        /// <param name="executionDate">The date when the job should be executed.</param>
        public DelayedTrigger(DateTimeOffset executionDate)
        {
            NextFireDate = executionDate;
        }

        /// <inheritdoc />
        public ValueTask<Result> BeforeExecutionAsync(IJobContext context, CancellationToken ct = default)
        {
            if (NextFireDate is null)
            {
                return ValueTask.FromResult<Result>(new InvalidOperationError("Cannot execute this trigger twice."));
            }

            NextFireDate = null;
            return ValueTask.FromResult(Result.FromSuccess());
        }

        /// <inheritdoc />
        public ValueTask<Result> AfterExecutionAsync
            (IJobContext context, Result jobResult, CancellationToken ct = default)
            => ValueTask.FromResult(Result.FromSuccess());

        /// <inheritdoc />
        public ValueTask<bool> CanBeExecutedAsync() => ValueTask.FromResult(true);

        /// <inheritdoc />
        public ValueTask RegisterReadyCallbackAsync(Func<Task> readyTask) => ValueTask.CompletedTask;

        /// <inheritdoc />
        public DateTimeOffset? NextFireDate { get; private set; }
    }
}