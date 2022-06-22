//
//  StringExtensions.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Enhancements.Extensions;

/// <summary>
/// Extensions for <see cref="string"/>.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Shorten the content to the maximum value.
    /// </summary>
    /// <param name="content">The content to shorten.</param>
    /// <param name="maxLength">The maximum length.</param>
    /// <returns>The shortened string.</returns>
    public static string Shorten(this string content, int maxLength)
    {
        if (content.Length > 100)
        {
            return content.Substring(0, maxLength - content.Length);
        }

        return content;
    }
}