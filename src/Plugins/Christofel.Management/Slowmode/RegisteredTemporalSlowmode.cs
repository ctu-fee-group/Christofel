//
//   RegisteredTemporalSlowmode.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using Christofel.Management.Database.Models;
using Christofel.Scheduling;

namespace Christofel.Management.Slowmode
{
    /// <summary>
    /// Temporal slowmode that is registered for disable when DeactivationTime is reached.
    /// </summary>
    /// <param name="TemporalSlowmodeEntity">The entity that represents the slowmode.</param>
    /// <param name="JobDescriptor">The descriptor of the job.</param>
    public record RegisteredTemporalSlowmode
        (TemporalSlowmode TemporalSlowmodeEntity, IJobDescriptor JobDescriptor);
}