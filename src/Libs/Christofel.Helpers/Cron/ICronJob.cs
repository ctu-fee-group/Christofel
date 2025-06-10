//
//   ICronJob.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Christofel.Plugins.Runtime;
using Remora.Results;

namespace Christofel.Helpers.Cron;

/// <summary>
/// A recurrable job.
/// </summary>
public interface ICronJob : IStartable, IStoppable
{
    /// <summary>
    /// Trigger the cron job, now, without waiting.
    /// </summary>
    /// <returns>A task representing the job.</returns>
    Task<IResult> TriggerNowAsync();

    /// <summary>
    /// Reschedule the job for the given datetime instead of the current <see cref="NextScheduledTime"/>.
    /// </summary>
    /// <param name="date">The time to run at.</param>
    /// <returns>A task representing the job to reschedule.</returns>
    Task<IResult> RescheduleNextAsync(DateTime date);

    /// <summary>
    /// Gets when is the next time this cron job will run.
    /// </summary>
    DateTime NextScheduledTime { get; }

    /// <summary>
    /// Gets the interval this cron is scheduled between.
    /// </summary>
    TimeSpan Interval { get; }
}
