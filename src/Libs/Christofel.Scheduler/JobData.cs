//
//  JobData.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Christofel.Scheduler.Abstractions;

namespace Christofel.Scheduler
{
    /// <inheritdoc cref="IJobData"/>
    public record JobData
        (Type JobType, IReadOnlyDictionary<string, object> Data, JobKey Key, IJob? JobInstance = null) : IJobData;
}