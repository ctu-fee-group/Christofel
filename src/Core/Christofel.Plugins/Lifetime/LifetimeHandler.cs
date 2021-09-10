//
//   LifetimeHandler.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;

namespace Christofel.Plugins.Lifetime
{
#pragma warning disable SA1402 // FileMayOnlyContainASingleType

    /// <summary>
    /// Manages <see cref="ILifetime"/> state and calling events when needed.
    /// </summary>
    public abstract class LifetimeHandler : IDisposable
    {
        private Action<Exception?>? _errorAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="LifetimeHandler"/> class.
        /// </summary>
        /// <param name="errorAction">Action that will be called when moving to error state.</param>
        protected LifetimeHandler(Action<Exception?> errorAction)
        {
            _errorAction = errorAction;
            Started = new CancellationTokenSource();
            Stopped = new CancellationTokenSource();
            Stopping = new CancellationTokenSource();
            Errored = new CancellationTokenSource();
        }

        /// <summary>
        /// Gets whether there was an irrecoverable error.
        /// </summary>
        public bool IsErrored { get; private set; }

        /// <summary>
        /// Gets the lifetime state that is managed by this handler.
        /// </summary>
        public abstract ILifetime Lifetime { get; }

        /// <summary>
        /// Source for the <see cref="ILifetime.Started"/> cancellation token.
        /// </summary>
        protected CancellationTokenSource Started { get; }

        /// <summary>
        /// Source for the <see cref="ILifetime.Stopped"/> cancellation token.
        /// </summary>
        protected CancellationTokenSource Stopped { get; }

        /// <summary>
        /// Source for the <see cref="ILifetime.Stopping"/> cancellation token.
        /// </summary>
        protected CancellationTokenSource Stopping { get; }

        /// <summary>
        /// Source for the <see cref="ILifetime.Errored"/> cancellation token.
        /// </summary>
        protected CancellationTokenSource Errored { get; }

        /// <summary>
        /// Gets lock that is used for moving the lifetime to next states.
        /// </summary>
        protected object NextStateLock { get; } = new object();

        /// <inheritdoc />
        public virtual void Dispose()
        {
            Started.Dispose();
            Stopped.Dispose();
            Stopping.Dispose();
            Errored.Dispose();

            _errorAction = null;
        }

        /// <summary>
        /// Moves lifetime to the specified state.
        /// </summary>
        /// <param name="state">What state to move to.</param>
        public abstract void MoveToState(LifetimeState state);

        /// <summary>
        /// Requests stop of the object the lifetime represents.
        /// </summary>
        public abstract void RequestStop();

        /// <summary>
        /// Moves to the next state after the current one.
        /// </summary>
        public virtual void NextState()
        {
            LifetimeState next;
            lock (NextStateLock)
            {
                if (Lifetime.State == LifetimeState.Destroyed)
                {
                    return;
                }

                next = Lifetime.State + 1;
                MoveToState(next);
            }
        }

        /// <summary>
        /// Triggers events of the lifetime if it is needed.
        /// </summary>
        /// <param name="previous">What was the previous state the next state replaces.</param>
        /// <param name="next">What is the new state of the lifetime.</param>
        protected void TriggerStateActions(LifetimeState previous, LifetimeState next)
        {
            if (previous == next)
            {
                return;
            }

            switch (next)
            {
                case LifetimeState.Running:
                    HandleStarted();
                    break;
                case LifetimeState.Stopped:
                    HandleStopped();
                    break;
                case LifetimeState.Stopping:
                    HandleStopping();
                    break;
            }
        }

        /// <summary>
        /// Immediately moves to error state.
        /// </summary>
        /// <remarks>
        /// Sets IsErrored to true and cancels Errored cancellation token.
        /// </remarks>
        /// <param name="e">What exception led to the error, if any.</param>
        public virtual void MoveToError(Exception? e)
        {
            if (!IsErrored)
            {
                IsErrored = true;
                _errorAction?.Invoke(e);
                Errored.Cancel();
            }
        }

        /// <summary>
        /// Cancels errored cancellation token.
        /// </summary>
        protected void HandleErrored()
        {
            Errored.Cancel(false);
        }

        /// <summary>
        /// Cancels started cancellation token.
        /// </summary>
        protected void HandleStarted()
        {
            Started.Cancel(false);
        }

        /// <summary>
        /// Cancels stopping cancellation token.
        /// </summary>
        protected void HandleStopping()
        {
            Stopping.Cancel(false);
        }

        /// <summary>
        /// Cancels stopped cancellation token.
        /// </summary>
        protected void HandleStopped()
        {
            Stopped.Cancel(false);
        }
    }

    /// <summary>
    /// Manages <see cref="ILifetime{T}"/> state and calling all events when needed.
    /// </summary>
    /// <remarks>
    /// Holds specified generic type of lifetime instead of the base non-generic type.
    /// Useful for working with dependency injection.
    /// </remarks>
    /// <typeparam name="T">Type of the lifetime.</typeparam>
    public abstract class LifetimeHandler<T> : LifetimeHandler
        where T : ILifetime
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LifetimeHandler{T}"/> class.
        /// </summary>
        /// <param name="errorAction">Action that will be called when moving to error state.</param>
        protected LifetimeHandler(Action<Exception?> errorAction)
            : base(errorAction)
        {
        }

        /// <inheritdoc />
        public override ILifetime Lifetime => LifetimeSpecific;

        /// <summary>
        /// Same as <see cref="Lifetime"/>, but the specified generic type.
        /// </summary>
        public abstract T LifetimeSpecific { get; }
    }
}