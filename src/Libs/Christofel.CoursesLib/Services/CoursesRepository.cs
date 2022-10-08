//
//  CoursesRepository.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using Christofel.Common.Database;
using Christofel.Common.User;
using Christofel.CoursesLib.Database;
using Christofel.CoursesLib.Extensions;
using Kos.Abstractions;
using Kos.Data;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.CoursesLib.Services;

/// <summary>
/// A class for obtaining information about departments and courses.
/// </summary>
public class CoursesRepository
{
    private readonly IReadableDbContext<CoursesContext> _coursesContext;
    private readonly IKosStudentsApi _studentsApi;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoursesRepository"/> class.
    /// </summary>
    /// <param name="coursesContext">The courses database context.</param>
    /// <param name="studentsApi">The students api.</param>
    public CoursesRepository(IReadableDbContext<CoursesContext> coursesContext, IKosStudentsApi studentsApi)
    {
        _coursesContext = coursesContext;
        _studentsApi = studentsApi;
    }

    /// <summary>
    /// Gets a list of all of the departments.
    /// </summary>
    /// <param name="includeCourses">Whether to include list courses.</param>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>All of the departments or an error.</returns>
    public async Task<Result<IReadOnlyList<DepartmentAssignment>>> GetDepartments
        (bool includeCourses, CancellationToken ct = default)
    {
        try
        {
            var set = _coursesContext.Set<DepartmentAssignment>();

            if (includeCourses)
            {
                set = set.Include(x => x.Courses);
            }

            return await set.ToListAsync(ct);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    /// <summary>
    /// Gets multiple courses by the given keys.
    /// </summary>
    /// <param name="ct">The cancellation token to cancel the operation.</param>
    /// <param name="courseKeys">The keys of courses to find.</param>
    /// <returns>A course with the given key or an error.</returns>
    public async Task<Result<IReadOnlyList<CourseAssignment>>> GetCourseAssignments
        (CancellationToken ct = default, params string[] courseKeys)
    {
        try
        {
            return await _coursesContext.Set<CourseAssignment>()
                .Include(x => x.Department)
                .Include(x => x.GroupAssignment)
                .Where(x => courseKeys.Contains(x.CourseKey))
                .ToListAsync(ct);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    /// <summary>
    /// Gets multiple courses by the given search keys such as part of name or key.
    /// </summary>
    /// <param name="ct">The cancellation token to cancel the operation.</param>
    /// <param name="searchKeys">Parts of key or name of the course to search..</param>
    /// <returns>A course with the given key or an error.</returns>
    public async Task<Result<IReadOnlyList<CourseAssignment>>> SearchCourseAssignments
        (CancellationToken ct = default, params string[] searchKeys)
    {
        searchKeys = searchKeys.Select(x => x.ToLower()).ToArray();

        try
        {
            var predicates = searchKeys.Select
            (
                k => (Expression<Func<CourseAssignment, bool>>)(x
                    => x.CourseKey.Contains(k) || x.CourseName.Contains(k) || (x.ChannelName != null && x.ChannelName.Contains(k)))
            );

            var courseAssignments = await _coursesContext.Set<CourseAssignment>()
                .Include(x => x.Department)
                .Include(x => x.GroupAssignment)
                .WhereAny(predicates.ToArray())
                .ToListAsync(ct);

            return courseAssignments;
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
    public async Task<Result<IReadOnlyList<CourseAssignment>>> GetCoursesByDepartment
        (string departmentKey, CancellationToken ct = default)
    {
        try
        {
            return await _coursesContext.Set<CourseAssignment>()
                .Include(x => x.GroupAssignment)
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
    public async Task<Result<IReadOnlyList<CourseAssignment>>> GetCoursesByChannel
        (Snowflake channelId, CancellationToken ct = default)
    {
        try
        {
            return await _coursesContext.Set<CourseAssignment>()
                .Include(x => x.GroupAssignment)
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
    public async Task<Result<IReadOnlyList<CourseAssignment>>> GetSemesterCourses
        (ICtuUser ctuUser, string semesterSelector, CancellationToken ct = default)
    {
        var enrolledCourses = await _studentsApi.GetStudentEnrolledCourses
            (ctuUser.CtuUsername, semesterSelector, limit: 100, token: ct);
        var courseKeys = enrolledCourses
            .OfType<InternalCourseEnrollment>()
            .Where(x => x.Course is not null)
            .Select(x => x.Course!.GetKey())
            .ToArray();

        return await _coursesContext.Set<CourseAssignment>()
            .Where(x => courseKeys.Contains(x.CourseKey))
            .ToListAsync(ct);
    }

    /// <summary>
    /// Gets a list of all course keys the user is enrolled in for the given semester.
    /// </summary>
    /// <param name="ctuUser">The ctu user.</param>
    /// <param name="semesterSelector">The semester selector.</param>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>All of the courses the user is enrolled to.</returns>
    public async Task<Result<IReadOnlyList<string>>> GetSemesterCoursesKeys
        (ICtuUser ctuUser, string semesterSelector, CancellationToken ct = default)
    {
        var enrolledCourses = await _studentsApi.GetStudentEnrolledCourses
            (ctuUser.CtuUsername, semesterSelector, limit: 100, token: ct);

        return enrolledCourses
            .OfType<InternalCourseEnrollment>()
            .Where(x => x.Course is not null)
            .Select(x => x.Course!.GetKey())
            .ToArray();
    }
}