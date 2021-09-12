//
//   IJobData.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Christofel.Scheduler.Abstractions
{
    /// <summary>
    /// Information about job that should be instantiated.
    /// </summary>
    public interface IJobData
    {
        /// <summary>
        /// Gets instance of the job if it was passed in already instantiated.
        /// </summary>
        public IJob? JobInstance { get; }

        /// <summary>
        /// Gets the type of the job to be instantiated.
        /// </summary>
        public Type JobType { get; }

        /// <summary>
        /// Gets data to be passed to the job.
        /// </summary>
        public IReadOnlyDictionary<string, object> Data { get; }

        /// <summary>
        /// Gets the key of the job.
        /// </summary>
        public JobKey Key { get; }
    }
}