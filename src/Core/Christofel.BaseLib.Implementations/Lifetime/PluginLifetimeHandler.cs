using System;
using System.Threading;
using Christofel.BaseLib.Plugins;

namespace Christofel.BaseLib.Lifetime
{
    /// <summary>
    /// LifetimeHandler for a plugin
    /// </summary>
    public class PluginLifetimeHandler : LifetimeHandler<ICurrentPluginLifetime>
    {
        private Action? _handleStopRequest;
        private PluginLifetime? _lifetime;
        private bool _stopRequestReceived;
        
        public PluginLifetimeHandler(Action<Exception?> handleError, Action handleStopRequest)
            : base (handleError)
        {
            State = LifetimeState.Startup;
            _handleStopRequest = handleStopRequest;
        }

        public LifetimeState State { get; private set; }

        public override ICurrentPluginLifetime LifetimeSpecific
        {
            get
            {
                if (_lifetime == null)
                {
                    _lifetime = new PluginLifetime(this, _started, _stopping, _stopped, _errored);
                }

                return _lifetime;
            }
        }

        public override void MoveToState(LifetimeState state)
        {
            LifetimeState current;
            lock (_nextStateLock)
            {
                current = State;
                State = state;
            }

            TriggerStateActions(current, state);
        }

        public override void RequestStop()
        {
            if (!_stopRequestReceived)
            {
                _stopRequestReceived = true;
                _handleStopRequest?.Invoke();
            }
        }

        public override void Dispose()
        {
            _handleStopRequest = null;
            base.Dispose();
        }

        public class PluginLifetime : ICurrentPluginLifetime
        {
            private readonly CancellationTokenSource _started, _stopped, _stopping, _errored;
            private readonly PluginLifetimeHandler _handler;

            public PluginLifetime(
                PluginLifetimeHandler handler,
                CancellationTokenSource started,
                CancellationTokenSource stopping,
                CancellationTokenSource stopped, 
                CancellationTokenSource errored)
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