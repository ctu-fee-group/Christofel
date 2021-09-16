//
//   SimpleTrigger.cs
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
    /// Simply triggers the job right away.
    /// </summary>
    public class SimpleTrigger : ITrigger
    {
        /// <inheritdoc />
        public ValueTask<Result> BeforeExecutionAsync(IJobContext context, CancellationToken ct = default)
        {
            NextFireDate = null;
            return ValueTask.FromResult(Result.FromSuccess());
        }

        /// <inheritdoc />
        public ValueTask<Result> AfterExecutionAsync
            (IJobContext context, Result jobResult, CancellationToken ct = default)
            => ValueTask.FromResult<Result>(Result.FromSuccess());

        /// <inheritdoc />
        public ValueTask<bool> CanBeExecutedAsync() => ValueTask.FromResult(true);

        /// <inheritdoc />
        public ValueTask RegisterReadyCallbackAsync(Func<Task> readyTask) => ValueTask.CompletedTask;

        /// <inheritdoc />
        public DateTimeOffset? NextFireDate { get; private set; } = DateTimeOffset.UtcNow;
    }
}