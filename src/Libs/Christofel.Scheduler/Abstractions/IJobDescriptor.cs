//
//   IJobDescriptor.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Scheduler.Abstractions
{
    /// <summary>
    /// Descriptor of the given <see cref="IJob"/>.
    /// </summary>
    public interface IJobDescriptor
    {
        /// <summary>
        /// Gets the job.
        /// </summary>
        public IJobData JobData { get; }

        /// <summary>
        /// Gets the trigger that schedules the job.
        /// </summary>
        public ITrigger Trigger { get; }

        /// <summary>
        /// Gets the key of this descriptor.
        /// </summary>
        public JobKey Key { get; }
    }
}