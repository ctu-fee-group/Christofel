//
//   CoursesFormatter.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.CoursesLib.Database;
using Remora.Rest.Core;

namespace Christofel.Courses;

/// <summary>
/// Formats courses into a string.
/// </summary>
public static class CoursesFormatter
{
    /// <summary>
    /// Format the given courses into a message separated by line.
    /// </summary>
    /// <param name="courses">The courses to format.</param>
    /// <returns>The formatted string.</returns>
    public static string FormatCoursesMessage(IEnumerable<CourseAssignment> courses)
        => string.Join
        (
            '\n',
            courses
                .GroupBy(x => x.ChannelId)
                .Select(FormatCourse)
        );

    /// <summary>
    /// Format the given courses.
    /// </summary>
    /// <param name="courses">The courses to format.</param>
    /// <returns>The dictionary with the formatted courses grouped by channel id.</returns>
    public static IDictionary<Snowflake, string> FormatCourses(IEnumerable<CourseAssignment> courses)
        => courses
            .GroupBy(x => x.ChannelId)
            .ToDictionary(x => x.Key, FormatCourse);

    /// <summary>
    /// Format the given course.
    /// </summary>
    /// <param name="courseAssignmentGrouping">The courses grouped by the channel id snowflake.</param>
    /// <returns>The formatted course string.</returns>
    public static string FormatCourse(IGrouping<Snowflake, CourseAssignment> courseAssignmentGrouping)
    {
        var keys = string.Join(", ", courseAssignmentGrouping.Take(5).Select(x => x.CourseKey));
        if (courseAssignmentGrouping.Count() > 5)
        {
            keys += ", ...";
        }

        var first = courseAssignmentGrouping.First();
        var courseName = first.GroupAssignment?.Name ?? first.CourseName;

        return $"  **<#{courseAssignmentGrouping.Key}>** - {courseName} ({keys})";
    }

    /// <summary>
    /// Format the given course.
    /// </summary>
    /// <param name="courseAssignment">The course to format.</param>
    /// <returns>The formatted course string.</returns>
    public static string FormatCourse(CourseAssignment courseAssignment)
    {
        var courseName = courseAssignment.GroupAssignment?.Name ?? courseAssignment.CourseName;
        return $"  **<#{courseAssignment.ChannelId}>** - {courseName} ({courseAssignment.CourseKey})";
    }
}