namespace Christofel.Api.Ctu.JobQueue
{
    public interface IJobQueue<TJob>
    {
        void EnqueueJob(TJob job);
    }
}