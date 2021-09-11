//
//   SimpleTrigger.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
        private bool _executed;

        /// <inheritdoc />
        public ValueTask<Result> BeforeExecutionAsync(IJobContext context, CancellationToken ct = default)
        {
            _executed = true;
            return ValueTask.FromResult(Result.FromSuccess());
        }

        /// <inheritdoc />
        public ValueTask<Result> AfterExecutionAsync
            (IJobContext context, Result jobResult, CancellationToken ct = default)
            => ValueTask.FromResult<Result>(Result.FromSuccess());

        /// <inheritdoc />
        public bool ShouldBeExecuted() => !_executed;

        /// <inheritdoc />
        public bool CanBeDeleted() => _executed;
    }
}