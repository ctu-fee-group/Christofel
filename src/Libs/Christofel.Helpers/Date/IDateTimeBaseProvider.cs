//
//   IDateTimeBaseProvider.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Christofel.Helpers.Date;

/// <summary>
/// Provides current time.
/// </summary>
public interface IDateTimeBaseProvider
{
    /// <summary>
    /// Right now in UTC. DateTimeKind is set to Utc.
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Right now in system's timezone.
    /// </summary>
    DateTime SystemNow { get; }
}
