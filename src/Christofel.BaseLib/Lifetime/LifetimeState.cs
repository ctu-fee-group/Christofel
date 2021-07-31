namespace Christofel.BaseLib.Lifetime
{
    public enum LifetimeState
    {
        Startup,
        Initializing,
        Initialized,
        Starting,
        Running,
        Stopping,
        Stopped,
        Destroyed,
        Error,
    }
}