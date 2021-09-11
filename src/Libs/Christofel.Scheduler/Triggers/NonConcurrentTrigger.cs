//
//   NonConcurrentTrigger.cs
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
    /// Trigger that does not allow more than 1 given task at once.
    /// </summary>
    public class NonConcurrentTrigger : ITrigger
    {
        private readonly ITrigger _underlyingTrigger;
        private readonly State _nonConcurrentState;

        /// <summary>
        /// Initializes a new instance of the <see cref="NonConcurrentTrigger"/> class.
        /// </summary>
        /// <param name="underlyingTrigger">The trigger that holds the actual scheduling information.</param>
        /// <param name="nonConcurrentState">The state of the concurrence.</param>
        public NonConcurrentTrigger(ITrigger underlyingTrigger, State nonConcurrentState)
        {
            _nonConcurrentState = nonConcurrentState;
            _underlyingTrigger = underlyingTrigger;
        }

        /// <inheritdoc />
        public ValueTask<Result> BeforeExecutionAsync
            (IJobContext context, CancellationToken ct = default)
        {
            if (!_nonConcurrentState.SetRunning())
            {
                return ValueTask.FromResult<Result>
                    (new InvalidOperationError("The task cannot be scheduled, because there is one already running."));
            }

            _nonConcurrentState.SetRunning();

            return _underlyingTrigger.BeforeExecutionAsync(context, ct);
        }

        /// <inheritdoc />
        public ValueTask<Result> AfterExecutionAsync
            (IJobContext context, Result jobResult, CancellationToken ct = default)
        {
            _nonConcurrentState.SetFinished();
            return _underlyingTrigger.AfterExecutionAsync(context, jobResult, ct);
        }

        /// <inheritdoc />
        public bool ShouldBeExecuted()
        {
            if (_nonConcurrentState.IsRunning())
            {
                return false;
            }

            return _underlyingTrigger.ShouldBeExecuted();
        }

        /// <inheritdoc />
        public bool CanBeDeleted() => _underlyingTrigger.CanBeDeleted();

        /// <summary>
        /// State of <see cref="NonConcurrentTrigger"/> to distinguish what jobs are non concurrent.
        /// </summary>
        public class State
        {
            private readonly object _lock = new object();
            private bool _running;

            /// <summary>
            /// Sets the state to running.
            /// </summary>
            /// <returns>Whether the state could be set, false if already running.</returns>
            public bool SetRunning()
            {
                lock (_lock)
                {
                    var currentlyRunning = _running;
                    _running = true;
                    return !currentlyRunning;
                }

            }

            /// <summary>
            /// Sets the state to finished.
            /// </summary>
            public void SetFinished()
            {
                lock (_lock)
                {
                    _running = false;
                }
            }

            /// <summary>
            /// Gets if the task is running.
            /// </summary>
            /// <returns>Whether the task with the given state is running.</returns>
            public bool IsRunning()
            {
                lock (_lock)
                {
                    return _running;
                }
            }
        }
    }
}