//
//   IJobQueue.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Api.Ctu.JobQueue
{
    public interface IJobQueue<TJob>
    {
        void EnqueueJob(TJob job);
    }
}