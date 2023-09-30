//
//   CoursesAssignmentOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Courses;

/// <summary>
/// Options for assigning courses.
/// </summary>
public class CoursesAssignmentOptions
{
    /// <summary>
    /// Gets or sets the courses to ignore, especially when these are missing.
    /// </summary>
    public IList<string> IgnoreCourses { get; set; } = new List<string>();
}