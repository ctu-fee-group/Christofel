//
//   ChristofelLifetimeHandler.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Plugins.Lifetime;
using Microsoft.Extensions.Logging;

namespace Christofel.Application.State
{
    /// <summary>
    /// Handles lifetime of the application.
    /// </summary>
    public class ChristofelLifetimeHandler : LifetimeHandler<IApplicationLifetime>
    {
        private readonly ChristofelApp _app;
        private ApplicationLifetime? _lifetime;
        private bool _stopRequestReceived;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChristofelLifetimeHandler"/> class.
        /// </summary>
        /// <param name="handleError">Action that will be called on error.</param>
        /// <param name="app">The application.</param>
        public ChristofelLifetimeHandler(Action<Exception?> handleError, ChristofelApp app)
            : base(handleError)
        {
            _app = app;
        }

        /// <summary>
        /// The logger of the application.
        /// </summary>
        public ILogger? Logger { get; set; }

        /// <summary>
        /// The state of the lifetime.
        /// </summary>
        public LifetimeState State { get; private set; }

        /// <inheritdoc />
        public override IApplicationLifetime LifetimeSpecific
        {
            get
            {
                if (_lifetime == null)
                {
                    _lifetime = new ApplicationLifetime(this, Started, Stopping, Stopped, Errored);
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
            if (_stopRequestReceived)
            {
                return;
            }

            _stopRequestReceived = true;

            Task.Run
            (
                async () =>
                {
                    try
                    {
                        await _app.StopAsync();
                    }
                    catch (Exception e)
                    {
                        Logger?.LogError(e, "Got exception while stopping the app");
                    }

                    try
                    {
                        await _app.DestroyAsync();
                    }
                    catch (Exception e)
                    {
                        Logger?.LogError(e, "Got exception while destroying the app");
                    }
                }
            );
        }

        private class ApplicationLifetime : IApplicationLifetime
        {
            private readonly ChristofelLifetimeHandler _handler;
            private readonly CancellationTokenSource _started;
            private readonly CancellationTokenSource _stopped;
            private readonly CancellationTokenSource _stopping;
            private readonly CancellationTokenSource _errored;

            public ApplicationLifetime
            (
                ChristofelLifetimeHandler handler,
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

            public LifetimeState State => _handler.State;

            public bool IsErrored => _handler.IsErrored;

            public CancellationToken Errored => _errored.Token;

            public CancellationToken Started => _started.Token;

            public CancellationToken Stopped => _stopped.Token;

            public CancellationToken Stopping => _stopping.Token;

            public void RequestStop()
            {
                _handler.RequestStop();
            }
        }
    }
}