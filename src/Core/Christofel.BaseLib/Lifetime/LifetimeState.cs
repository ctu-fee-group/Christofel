namespace Christofel.BaseLib.Lifetime
{
    public enum LifetimeState
    {
        /// <summary>
        /// When service is created, startup should be the first state
        /// </summary>
        Startup,
        /// <summary>
        /// After calling some kind of initializer
        /// </summary>
        Initializing,
        /// <summary>
        /// After initializer has finished
        /// </summary>
        Initialized,
        /// <summary>
        /// After calling some kind of start or run function
        /// </summary>
        Starting,
        /// <summary>
        /// After starting was finished
        /// </summary>
        Running,
        /// <summary>
        /// When Stop is requested (<see cref="ILifetime.RequestStop"/>)
        /// </summary>
        Stopping,
        /// <summary>
        /// After everything is stopped
        /// </summary>
        Stopped,
        /// <summary>
        /// After destroy of every object that belonged to the plugin
        /// </summary>
        Destroyed,
    }
}