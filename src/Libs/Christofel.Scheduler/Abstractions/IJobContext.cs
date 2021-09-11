//
//   IJobContext.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Scheduler.Abstractions
{
    /// <summary>
    /// Context for <see cref="IJob"/>.
    /// </summary>
    public interface IJobContext
    {
        /// <summary>
        /// Gets the descriptor of the job.
        /// </summary>
        public IJobDescriptor JobDescriptor { get; }

        /// <summary>
        /// Gets the job that is being executed.
        /// </summary>
        public IJob Job { get; }

        /// <summary>
        /// Gets the trigger that is associated with the given job.
        /// </summary>
        public ITrigger Trigger { get; }
    }
}