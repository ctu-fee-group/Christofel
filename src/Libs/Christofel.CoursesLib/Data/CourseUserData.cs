//
//  CourseUserData.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.CoursesLib.Database;

namespace Christofel.CoursesLib.Data;

/// <summary>
/// Association of a user with given course.
/// A user can be a member of a course.
/// </summary>
/// <param name="Course">The course assignment data.</param>
/// <param name="IsMember">Whether the user is member of the given course.</param>
public record CourseUserData(CourseAssignment Course, bool IsMember);
