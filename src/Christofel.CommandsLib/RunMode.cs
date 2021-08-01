namespace Christofel.CommandsLib
{
    public enum RunMode
    {
        /// <summary>
        /// Run command in same thread
        /// </summary>
        SameThread,
        /// <summary>
        /// Run command in new thread obtained from ThreadPool (Task.Run may be used)
        /// </summary>
        NewThread,
    }
}