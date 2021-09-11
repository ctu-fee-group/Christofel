//
//   IDataJob.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Scheduler.Abstractions;

namespace Christofel.Scheduler.Recoverable
{
    /// <summary>
    /// Job with data.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    public interface IDataJob<TData> : IJob
    {
        /// <summary>
        /// Gets or sets the data of the job.
        /// </summary>
        public TData Data { get; set; }
    }
}