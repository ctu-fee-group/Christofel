using System;
using System.Threading;

namespace Christofel.BaseLib.Lifetime
{
    public abstract class LifetimeHandler
    {
        protected readonly CancellationTokenSource _started, _stopped, _stopping, _errored;
        protected readonly Action<Exception?> _errorAction;
        protected object _nextStateLock = new object();

        protected LifetimeHandler(Action<Exception?> errorAction)
        {
            _errorAction = errorAction;
            _started = new CancellationTokenSource();
            _stopped = new CancellationTokenSource();
            _stopping = new CancellationTokenSource();
            _errored = new CancellationTokenSource();
        }

        public bool IsErrored => Lifetime.State == LifetimeState.Error;

        public abstract ILifetime Lifetime { get; }

        public abstract void MoveToState(LifetimeState state);

        public abstract void RequestStop();

        public virtual void NextState()
        {
            LifetimeState next;
            lock (_nextStateLock)
            {

                if (Lifetime.State == LifetimeState.Destroyed)
                {
                    return;
                }

                next = Lifetime.State + 1;
                MoveToState(next);
            }
            
        }

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
                case LifetimeState.Error:
                    HandleErrored();
                    break;
            }
        }

        public virtual void MoveToError(Exception? e)
        {
            _errorAction(e);
            _errored.Cancel();
            MoveToState(LifetimeState.Error);
        }
        
        public void HandleErrored()
        {
            _errored.Cancel(false);
        }

        public void HandleStarted()
        {
            _started.Cancel(false);
        }

        public void HandleStopping()
        {
            _stopping.Cancel(false);
        }

        public void HandleStopped()
        {
            _stopped.Cancel(false);
        }
    }
    
    public abstract class LifetimeHandler<T> : LifetimeHandler
        where T : ILifetime
    {
        protected LifetimeHandler(Action<Exception?> errorAction)
            : base(errorAction)
        {
        }
        
        public override ILifetime Lifetime => LifetimeSpecific;

        public abstract T LifetimeSpecific { get; }
    }
}