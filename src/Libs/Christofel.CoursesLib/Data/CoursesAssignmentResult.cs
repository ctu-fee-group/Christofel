//
//  CoursesAssignmentResult.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.CoursesLib.Database;
using Remora.Results;

namespace Christofel.CoursesLib.Data;

/// <summary>
/// A result for deassigning/assigning
/// </summary>
/// <param name="AssignedCourses"></param>
/// <param name="DeassignedCourses"></param>
/// <param name="MissingCourses"></param>
/// <param name="ErrorfulResults"></param>
public record CoursesAssignmentResult
(
    IList<CourseAssignment> AssignedCourses,
    IList<CourseAssignment> DeassignedCourses,
    IList<string> MissingCourses,
    IReadOnlyDictionary<string, Result> ErrorfulResults
);
