namespace Christofel.Plugins.Lifetime
{
    public static class LifetimeHandlerExtensions
    {
        public static bool MoveToIfLower(this LifetimeHandler lifetimeHandler, LifetimeState state)
        {
            bool set = lifetimeHandler.Lifetime.State < state;

            if (set)
            {
                lifetimeHandler.MoveToState(state);
            }

            return set;
        }
        
        public static bool MoveToIfPrevious(this LifetimeHandler lifetimeHandler, LifetimeState state)
        {
            bool set = lifetimeHandler.Lifetime.State < state;

            if (set)
            {
                lifetimeHandler.MoveToState(state);
            }

            return set;
        }
    }
}