//
//   TypedJobData.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Christofel.Scheduler.Abstractions;

namespace Christofel.Scheduler
{
    /// <summary>
    /// The job data that are typed to ease initialization.
    /// </summary>
    /// <typeparam name="TJob">The type of the job.</typeparam>
    public class TypedJobData<TJob> : IJobData
        where TJob : IJob
    {
        private readonly Dictionary<string, object> _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypedJobData{TJob}"/> class.
        /// </summary>
        /// <param name="jobInstance">The instance of the job.</param>
        /// <param name="key">The key of the job.</param>
        public TypedJobData(JobKey key, TJob? jobInstance)
            : this(key)
        {
            JobInstance = jobInstance;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypedJobData{T}"/> class.
        /// </summary>
        /// <param name="key">The key of the job.</param>
        public TypedJobData(JobKey key)
        {
            Key = key;
            JobType = typeof(TJob);
            _data = new Dictionary<string, object>();
        }

        /// <inheritdoc />
        public IJob? JobInstance { get; }

        /// <inheritdoc />
        public Type JobType { get; }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, object> Data => _data;

        /// <inheritdoc />
        public JobKey Key { get; }

        /// <summary>
        /// Adds data to the dictionary.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="data">The value to add with the given key.</param>
        /// <returns>This.</returns>
        public TypedJobData<TJob> AddData(string key, object data)
        {
            _data.Add(key, data);
            return this;
        }
    }
}