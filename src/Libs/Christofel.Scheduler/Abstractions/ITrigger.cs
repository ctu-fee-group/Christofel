//
//   ITrigger.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
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
        /// <returns>Whether the job should be executed. If false, <see cref="RegisterReadyCallbackAsync"/> will be called.</returns>
        public ValueTask<bool> CanBeExecutedAsync();

        /// <summary>
        /// Registers callback that should be called when this trigger becomes ready to be executed.
        /// </summary>
        /// <remarks>
        /// This will be called only after <see cref="CanBeExecutedAsync"/> will return false.
        /// </remarks>
        /// <param name="readyTask">The action to be called when this trigger is ready.</param>
        /// <returns>A <see cref="ValueTask"/> that represents the asynchronous operation.</returns>
        public ValueTask RegisterReadyCallbackAsync(Func<Task> readyTask);

        /// <summary>
        /// Gets the next time this trigger should fire.
        /// </summary>
        /// <remarks>
        /// should be `null` if the trigger won't fire more times and it can be safely removed.
        /// </remarks>
        public DateTimeOffset? NextFireDate { get; }
    }
}