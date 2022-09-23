//
//  CurrentSemesterCacheOptions.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.CoursesLib.Data;

/// <summary>
/// Options for <see cref="CurrentSemesterCacheOptions"/>.
/// </summary>
public class CurrentSemesterCacheOptions
{
    /// <summary>
    /// Gets or sets the number of minutes to cache the semester for.
    /// </summary>
    public int CacheDuration { get; set; } = 1440;
}