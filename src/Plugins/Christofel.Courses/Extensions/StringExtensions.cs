//
//  StringExtensions.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Courses.Extensions;

/// <summary>
/// Extension methods for <see cref="string"/>.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Truncate the given string to given length.
    /// </summary>
    /// <param name="value">The string to truncate.</param>
    /// <param name="maxLength">The maximum length to truncate to.</param>
    /// <returns>A truncated string.</returns>
    public static string Truncate(this string value, int maxLength)
    {
        if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
        {
            return value.Substring(0, maxLength);
        }

        return value;
    }

}