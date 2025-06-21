//
//   IDateTimeProvider.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Christofel.Helpers.Date;

/// <summary>
/// Provides basic operations with dates, including managing preferred timezone.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Right now in UTC. DateTimeKind is set to Utc.
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Right now, but in system's timezone.
    /// </summary>
    DateTimeOffset SystemNow { get; }

    /// <summary>
    /// Right now, but in <see name="PreferredTimeZone" />.
    /// </summary>
    DateTimeOffset PreferredNow { get; }

    /// <summary>
    /// User's preferred time zone.
    /// </summary>
    TimeZoneInfo PreferredTimeZone { get; }
}
