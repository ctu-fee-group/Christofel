//
//   TimeOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Helpers.Date;

/// <summary>
/// Options to use for time, ie. timezone, locale...
/// </summary>
public class TimeOptions
{
    /// <summary>
    /// Gets or sets the time zone to use by default when necessary.
    /// This time zone should be shared by most users.
    /// </summary>
    public string TimeZone { get; set; } = null!;

    /// <summary>
    /// Gets or sets the culture to use for formatting DateTime.
    /// </summary>
    public string Culture { get; set; } = "en_US";
}
