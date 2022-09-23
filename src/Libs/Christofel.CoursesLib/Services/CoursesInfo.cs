//
//   CoursesInfo.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Common.Database;
using Christofel.Common.User;
using Christofel.CoursesLib.Database;
using Christofel.CoursesLib.Extensions;
using Kos.Abstractions;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.CoursesLib.Services;

/// <summary>
/// A class for obtaining information about departments and courses.
/// </summary>
public class CoursesInfo
{
    private readonly IReadableDbContext<CoursesContext> _coursesContext;
    private readonly IKosStudentsApi _studentsApi;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoursesInfo"/> class.
    /// </summary>
    /// <param name="coursesContext">The courses database context.</param>
    /// <param name="studentsApi">The students api.</param>
    public CoursesInfo(IReadableDbContext<CoursesContext> coursesContext, IKosStudentsApi studentsApi)
    {
        _coursesContext = coursesContext;
        _studentsApi = studentsApi;
    }

    /// <summary>
    /// Gets a list of all of the departments.
    /// </summary>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>All of the departments or an error.</returns>
    public async Task<Result<IList<DepartmentAssignment>>> GetDepartments(CancellationToken ct = default)
    {
        try
        {
            return await _coursesContext.Set<DepartmentAssignment>()
                .DistinctBy(x => x.DepartmentKey)
                .ToListAsync(ct);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    /// <summary>
    /// Gets a course by the given key.
    /// </summary>
    /// <param name="courseKey">The key of the course.</param>
    /// <returns>A course with the given key or an error.</returns>
    public async Task<Result<CourseAssignment>> GetCourseAssignment(string courseKey)
    {
        try
        {
            var courseAssignment = await _coursesContext.Set<CourseAssignment>()
                .FirstOrDefaultAsync(x => x.CourseKey == courseKey);

            if (courseAssignment is null)
            {
                return new NotFoundError("Could not find the given course.");
            }

            return courseAssignment;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    /// <summary>
    /// Gets a list of all courses that belong to the given department.
    /// </summary>
    /// <param name="departmentKey">The key of the department.</param>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>All of the courses belonging to the department or an error.</returns>
    public async Task<Result<IList<CourseAssignment>>> GetCoursesByDepartment(string departmentKey, CancellationToken ct = default)
    {
        try
        {
            return await _coursesContext.Set<CourseAssignment>()
                .Where(x => x.DepartmentKey == departmentKey)
                .ToListAsync(ct);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    /// <summary>
    /// Gets a list of all courses for given channel.
    /// </summary>
    /// <param name="channelId">The course channel.</param>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>All of the courses linked to the given channel or an error.</returns>
    public async Task<Result<IList<CourseAssignment>>> GetCoursesByChannel
        (Snowflake channelId, CancellationToken ct = default)
    {
        try
        {
            return await _coursesContext.Set<CourseAssignment>()
                .Where(x => x.ChannelId == channelId)
                .ToListAsync(ct);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    /// <summary>
    /// Gets a list of all courses the user is enrolled in for the given semester.
    /// </summary>
    /// <param name="ctuUser">The ctu user.</param>
    /// <param name="semesterSelector">The semester selector.</param>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>All of the courses the user is enrolled to.</returns>
    public async Task<Result<IList<CourseAssignment>>> GetSemesterCourses(ICtuUser ctuUser, string semesterSelector, CancellationToken ct = default)
    {
        var enrolledCourses = await _studentsApi.GetStudentEnrolledCourses
            (ctuUser.CtuUsername, semesterSelector, limit: 100, token: ct);
        var courseKeys = enrolledCourses.Select(x => x.Course.GetKey()).ToArray();

        return await _coursesContext.Set<CourseAssignment>()
            .Where(x => courseKeys.Contains(x.CourseKey))
            .ToListAsync(ct);
    }
}