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
                .Select(x => FormatCourse(x).Formatted)
        );

    /// <summary>
    /// Format the given courses.
    /// </summary>
    /// <param name="courses">The courses to format.</param>
    /// <returns>The dictionary with the formatted courses grouped by channel id.</returns>
    public static IDictionary<Snowflake, (string Name, string Formatted)> FormatCourses
        (IEnumerable<CourseAssignment> courses)
        => courses
            .GroupBy(x => x.ChannelId)
            .ToDictionary(x => x.Key, FormatCourse);

    /// <summary>
    /// Format the given course.
    /// </summary>
    /// <param name="courseAssignmentGrouping">The courses grouped by the channel id snowflake.</param>
    /// <returns>The formatted course string.</returns>
    public static (string Name, string Formatted) FormatCourse
        (IGrouping<Snowflake, CourseAssignment> courseAssignmentGrouping)
    {
        var keys = string.Join(", ", courseAssignmentGrouping.Take(5).Select(x => x.CourseKey));
        if (courseAssignmentGrouping.Count() > 5)
        {
            keys += ", ...";
        }

        var first = courseAssignmentGrouping.First();
        var channelName = courseAssignmentGrouping
            .FirstOrDefault(x => x.ChannelName is not null)?.ChannelName;
        var courseName = first.GroupAssignment?.Name ?? first.CourseName;

        if (channelName is not null)
        {
            return (courseName, $"  **#{channelName}** (<#{courseAssignmentGrouping.Key}>) - {courseName} ({keys})");
        }

        return (courseName, $"  <#{courseAssignmentGrouping.Key}> - {courseName} ({keys})");
    }

    /// <summary>
    /// Format the given course.
    /// </summary>
    /// <param name="courseAssignment">The course to format.</param>
    /// <returns>The formatted course string.</returns>
    public static (string Name, string Formatted) FormatCourse(CourseAssignment courseAssignment)
    {
        var courseName = courseAssignment.GroupAssignment?.Name ?? courseAssignment.CourseName;
        var channelName = courseAssignment.ChannelName;
        if (channelName is not null)
        {
            return (courseName, $"  **#{channelName}** (<#{courseAssignment.ChannelId}>) - {courseName} ({courseAssignment.CourseKey})");
        }

        return (courseName, $"  <#{courseAssignment.ChannelId}> - {courseName} ({courseAssignment.CourseKey})");
    }
}