using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib;
using Christofel.BaseLib.Lifetime;
using Christofel.BaseLib.Plugins;
using Microsoft.Extensions.Logging;

namespace Christofel.Application.State
{
    /// <summary>
    /// Handles lifetime of the application
    /// </summary>
    public class ChristofelLifetimeHandler : LifetimeHandler<IApplicationLifetime>
    {
        private ApplicationLifetime? _lifetime;
        private readonly ChristofelApp _app;
        private bool _stopRequestReceived;

        public ChristofelLifetimeHandler(Action<Exception?> handleError, ChristofelApp app)
            : base(handleError)
        {
            _app = app;
        }
        
        public ILogger? Logger { get; set; }
        
        public LifetimeState State { get; private set; }

        public override IApplicationLifetime LifetimeSpecific
        {
            get
            {
                if (_lifetime == null)
                {
                    _lifetime = new ApplicationLifetime(this, _started, _stopping, _stopped, _errored);
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
            if (_stopRequestReceived)
            {
                return;
            }
            
            _stopRequestReceived = true;
            
            Task.Run(async () =>
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
            });
        }

        public class ApplicationLifetime : IApplicationLifetime
        {
            private readonly CancellationTokenSource _started, _stopped, _stopping, _errored;
            private readonly ChristofelLifetimeHandler _handler;

            public ApplicationLifetime(
                ChristofelLifetimeHandler handler,
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