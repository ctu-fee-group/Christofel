//
//   CoursesInfo.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Common.Database;
using Christofel.CoursesLib.Database;
using Microsoft.EntityFrameworkCore;
using Remora.Results;

namespace Christofel.CoursesLib.Services;

/// <summary>
/// A class for obtaining information about departments and courses.
/// </summary>
public class CoursesInfo
{
    private readonly IReadableDbContext<CoursesContext> _coursesContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoursesInfo"/> class.
    /// </summary>
    /// <param name="coursesContext">The courses database context.</param>
    public CoursesInfo(IReadableDbContext<CoursesContext> coursesContext)
    {
        _coursesContext = coursesContext;
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
}