//
//   JobKeyUtils.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Christofel.Scheduler.Abstractions;

namespace Christofel.Scheduler
{
    /// <summary>
    /// Utils for generating <see cref="JobKey"/>.
    /// </summary>
    public static class JobKeyUtils
    {
        /// <summary>
        /// Generates job with random key for the given group.
        /// </summary>
        /// <param name="group">The group the job key should be in.</param>
        /// <param name="namePrefix">The prefix for the name.</param>
        /// <returns>The generated job key.</returns>
        public static JobKey GenerateRandom
            (string group, string namePrefix = "") => new JobKey(group, namePrefix + Guid.NewGuid().ToString());
    }
}