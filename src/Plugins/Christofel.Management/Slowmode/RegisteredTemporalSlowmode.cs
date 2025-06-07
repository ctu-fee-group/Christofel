//
//   RegisteredTemporalSlowmode.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using Christofel.Management.Database.Models;

namespace Christofel.Management.Slowmode
{
    /// <summary>
    /// Temporal slowmode that is registered for disable when DeactivationTime is reached.
    /// </summary>
    /// <param name="TemporalSlowmodeEntity">The entity that represents the slowmode.</param>
    /// <param name="CancellationTokenSource">The cancellation token for the operation that will disable the slowmode.</param>
    public record RegisteredTemporalSlowmode
        (TemporalSlowmode TemporalSlowmodeEntity, CancellationTokenSource CancellationTokenSource);
}