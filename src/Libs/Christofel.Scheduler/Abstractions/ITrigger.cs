//
//   ITrigger.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Remora.Results;

namespace Christofel.Scheduler.Abstractions
{
    /// <summary>
    /// Trigger for <see cref="IJob"/> that schedules the times the job should be executed.
    /// </summary>
    public interface ITrigger : IJobListener
    {
        /// <summary>
        /// Gets whether the job should be executed right now.
        /// </summary>
        /// <returns>Whether the job should be executed.</returns>
        public bool ShouldBeExecuted();

        /// <summary>
        /// Gets whether the trigger is not needed anymore and can be deleted.
        /// </summary>
        /// <remarks>
        /// Will be called after AfterExecutionAsync is called.
        /// </remarks>
        /// <returns>Whether this trigger should be deleted.</returns>
        public bool CanBeDeleted();
    }
}