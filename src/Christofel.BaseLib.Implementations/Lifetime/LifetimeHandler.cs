using System;
using System.Threading;

namespace Christofel.BaseLib.Lifetime
{
    /// <summary>
    /// LifetimeHandler for using some common methods that Lifetime needs to have
    /// </summary>
    public abstract class LifetimeHandler
    {
        protected readonly CancellationTokenSource _started, _stopped, _stopping, _errored;
        protected readonly Action<Exception?> _errorAction;
        protected object _nextStateLock = new object();
        
        /// <param name="errorAction">Action to be executed in case of an error</param>
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

        /// <summary>
        /// Moves to given state
        /// </summary>
        /// <param name="state">State to move to</param>
        public abstract void MoveToState(LifetimeState state);

        /// <summary>
        /// Requests stop
        /// </summary>
        public abstract void RequestStop();

        /// <summary>
        /// Move to next state from the current one
        /// </summary>
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

        /// <summary>
        /// Triggers actions for given state.
        /// </summary>
        /// <remarks>
        /// Cancels Started, Stopping, Stopped, Error if needed
        /// </remarks>
        /// <param name="previous"></param>
        /// <param name="next"></param>
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

        /// <summary>
        /// Immediatelly moves to error state
        /// </summary>
        /// <param name="e"></param>
        public virtual void MoveToError(Exception? e)
        {
            _errorAction(e);
            _errored.Cancel();
            MoveToState(LifetimeState.Error);
        }
        
        protected void HandleErrored()
        {
            _errored.Cancel(false);
        }

        protected void HandleStarted()
        {
            _started.Cancel(false);
        }

        protected void HandleStopping()
        {
            _stopping.Cancel(false);
        }

        protected void HandleStopped()
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