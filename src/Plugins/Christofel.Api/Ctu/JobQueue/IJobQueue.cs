//
//   IJobQueue.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Api.Ctu.JobQueue
{
    /// <summary>
    /// Represents generic job queue for assigning jobs.
    /// </summary>
    /// <typeparam name="TJob">The type of the job.</typeparam>
    public interface IJobQueue<TJob>
    {
        /// <summary>
        /// Enqueues the job to the job queue.
        /// </summary>
        /// <param name="job">The job to enqueue.</param>
        void EnqueueJob(TJob job);
    }
}