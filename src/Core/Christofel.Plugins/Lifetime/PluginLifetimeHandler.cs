//
//   PluginLifetimeHandler.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;

namespace Christofel.Plugins.Lifetime
{
    /// <summary>
    /// Manages <see cref="ICurrentPluginLifetime"/> state and calling events when needed.
    /// </summary>
    public class PluginLifetimeHandler : LifetimeHandler<ICurrentPluginLifetime>
    {
        private Action? _handleStopRequest;
        private PluginLifetime? _lifetime;
        private bool _stopRequestReceived;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginLifetimeHandler"/> class.
        /// </summary>
        /// <param name="handleError">Action that will be called when moving to error state.</param>
        /// <param name="handleStopRequest">Action that will be called for <see cref="RequestStop"/> to stop the held entity. It should not block, only request a stop.</param>
        public PluginLifetimeHandler(Action<Exception?> handleError, Action handleStopRequest)
            : base(handleError)
        {
            State = LifetimeState.Startup;
            _handleStopRequest = handleStopRequest;
        }

        /// <summary>
        /// Current state of the lifetime.
        /// </summary>
        public LifetimeState State { get; private set; }

        /// <inheritdoc />
        public override ICurrentPluginLifetime LifetimeSpecific
        {
            get
            {
                if (_lifetime == null)
                {
                    _lifetime = new PluginLifetime(this, Started, Stopping, Stopped, Errored);
                }

                return _lifetime;
            }
        }

        /// <inheritdoc />
        public override void MoveToState(LifetimeState state)
        {
            LifetimeState current;
            lock (NextStateLock)
            {
                current = State;
                State = state;
            }

            TriggerStateActions(current, state);
        }

        /// <inheritdoc />
        public override void RequestStop()
        {
            if (!_stopRequestReceived)
            {
                _stopRequestReceived = true;
                _handleStopRequest?.Invoke();
            }
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            _handleStopRequest = null;
            base.Dispose();
        }

        /// <summary>
        /// Lifetime of the plugin held by <see cref="PluginLifetimeHandler"/>.
        /// </summary>
        public class PluginLifetime : ICurrentPluginLifetime
        {
            private readonly PluginLifetimeHandler _handler;

            private readonly CancellationTokenSource _started;
            private readonly CancellationTokenSource _stopped;
            private readonly CancellationTokenSource _stopping;
            private readonly CancellationTokenSource _errored;

            /// <summary>
            /// Initializes a new instance of the <see cref="PluginLifetime"/> class.
            /// </summary>
            /// <param name="handler">Handler that manages this lifetime.</param>
            /// <param name="started">Source for <see cref="Started"/> cancellation token.</param>
            /// <param name="stopping">Source for <see cref="Stopping"/> cancellation token.</param>
            /// <param name="stopped">Source for <see cref="Stopped"/> cancellation token.</param>
            /// <param name="errored">Source for <see cref="Errored"/> cancellation token.</param>
            public PluginLifetime
            (
                PluginLifetimeHandler handler,
                CancellationTokenSource started,
                CancellationTokenSource stopping,
                CancellationTokenSource stopped,
                CancellationTokenSource errored
            )
            {
                _started = started;
                _stopped = stopped;
                _stopping = stopping;
                _handler = handler;
                _errored = errored;
            }

            /// <inheritdoc />
            public LifetimeState State => _handler.State;

            /// <inheritdoc />
            public bool IsErrored => _handler.IsErrored;

            /// <inheritdoc />
            public CancellationToken Errored => _errored.Token;

            /// <inheritdoc />
            public CancellationToken Started => _started.Token;

            /// <inheritdoc />
            public CancellationToken Stopped => _stopped.Token;

            /// <inheritdoc />
            public CancellationToken Stopping => _stopping.Token;

            /// <inheritdoc />
            public void RequestStop()
            {
                _handler.RequestStop();
            }
        }
    }
}