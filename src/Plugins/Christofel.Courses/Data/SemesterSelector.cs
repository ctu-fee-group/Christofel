//
//   SemesterSelector.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Courses.Data;

/// <summary>
/// Selector of semester.
/// </summary>
public enum SemesterSelector
{
    /// <summary>
    /// Selector of the previous semester.
    /// </summary>
    Previous,

    /// <summary>
    /// Selector of the current semester.
    /// </summary>
    Current,

    /// <summary>
    /// Selectore of the next semester.
    /// </summary>
    Next
}