//
//   NonConcurrentTrigger.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Scheduler.Abstractions;
using Nito.AsyncEx;
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
        public async ValueTask<Result> BeforeExecutionAsync
            (IJobContext context, CancellationToken ct = default)
        {
            if (!await _nonConcurrentState.SetRunningAsync(this))
            {
                return new InvalidOperationError("The task cannot be executed, because there is one already running.");
            }

            await _nonConcurrentState.SetRunningAsync(this);
            return await _underlyingTrigger.BeforeExecutionAsync(context, ct);
        }

        /// <inheritdoc />
        public async ValueTask<Result> AfterExecutionAsync
            (IJobContext context, Result jobResult, CancellationToken ct = default)
        {
            await _nonConcurrentState.SetFinishedAsync(this);
            return await _underlyingTrigger.AfterExecutionAsync(context, jobResult, ct);
        }

        /// <inheritdoc />
        public async ValueTask<bool> CanBeExecutedAsync() => !await _nonConcurrentState.IsRunningAsync();

        /// <inheritdoc />
        public async ValueTask RegisterReadyCallbackAsync
            (Func<Task> readyTask) => await _nonConcurrentState.RegisterCallbackAsync(this, readyTask);

        /// <inheritdoc />
        public DateTimeOffset? NextFireDate => _underlyingTrigger.NextFireDate;

        /// <summary>
        /// State of <see cref="NonConcurrentTrigger"/> to distinguish what jobs are non concurrent.
        /// </summary>
        public class State
        {
            private readonly AsyncLock _lock = new AsyncLock();

            private List<(object Owner, Func<Task> Action)> _callbacks =
                new List<(object Owner, Func<Task> Action)>();

            private object? _lockOwner;

            /// <summary>
            /// Sets the state to running.
            /// </summary>
            /// <param name="owner">The owner of the lock.</param>
            /// <returns>Whether the state could be set, false if already running.</returns>
            public async Task<bool> SetRunningAsync(object owner)
            {
                using (await _lock.LockAsync())
                {
                    if (_lockOwner is null)
                    {
                        _lockOwner = owner;
                        return true;
                    }

                    return false;
                }
            }

            /// <summary>
            /// Sets the state to finished.
            /// </summary>
            /// <param name="owner">The owner of the lock.</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            public async Task SetFinishedAsync(object owner)
            {
                using (await _lock.LockAsync())
                {
                    if (_lockOwner == owner)
                    {
                        _lockOwner = null;

                        if (_callbacks.Count > 0)
                        {
                            var first = _callbacks.First();
                            await first.Action.Invoke();
                            _callbacks.RemoveAt(0);
                        }
                    }
                }
            }

            /// <summary>
            /// Gets if the task is running.
            /// </summary>
            /// <returns>Whether the task with the given state is running.</returns>
            public async Task<bool> IsRunningAsync()
            {
                using (await _lock.LockAsync())
                {
                    return _lockOwner is not null;
                }
            }

            /// <summary>
            /// Registers the given callback to be called when the state is released.
            /// </summary>
            /// <param name="owner">The owner of the callback.</param>
            /// <param name="callback">The callback to be called.</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            public async Task RegisterCallbackAsync(object owner, Func<Task> callback)
            {
                using (await _lock.LockAsync())
                {
                    if (_lockOwner is null)
                    {
                        await callback.Invoke();
                    }
                    else
                    {
                        bool any = false;
                        foreach (var x in _callbacks)
                        {
                            if (x.Owner == owner)
                            {
                                any = true;
                                break;
                            }
                        }

                        if (!any)
                        {
                            _callbacks.Add((owner, callback));
                        }
                    }
                }
            }
        }
    }
}