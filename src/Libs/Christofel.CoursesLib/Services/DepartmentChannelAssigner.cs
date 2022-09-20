//
//  DepartmentChannelAssigner.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.CoursesLib.Database;
using Kos.Abstractions;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.CoursesLib.Services;

/// <summary>
/// A class for assigning categories to departments.
/// </summary>
public class DepartmentChannelAssigner
{
    private readonly CoursesContext _coursesContext;
    private readonly IKosBranchesApi _branchesApi;

    /// <summary>
    /// Initializes a new instance of the <see cref="DepartmentChannelAssigner"/> class.
    /// </summary>
    /// <param name="coursesContext">The courses database context.</param>
    /// <param name="branchesApi">The kos branches api.</param>
    public DepartmentChannelAssigner(CoursesContext coursesContext, IKosBranchesApi branchesApi)
    {
        _coursesContext = coursesContext;
        _branchesApi = branchesApi;
    }

    /// <summary>
    /// Assigns the given category to the department.
    /// </summary>
    /// <param name="departmentKey">The key of the department that will be assigned.</param>
    /// <param name="categoryId">The id of the category to assign.</param>
    /// <param name="ct">The cancellation token to cancel the operation.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    public async Task<Result> AssignDepartmentCategory
        (string departmentKey, Snowflake categoryId, CancellationToken ct = default)
    {
        var branch = await _branchesApi.GetBranch(departmentKey, token: ct);

        if (branch is null)
        {
            return new NotFoundError($"Could not find the given branch {departmentKey}.");
        }

        try
        {
            _coursesContext.Add
            (
                new DepartmentAssignment
                {
                    DepartmentKey = departmentKey,
                    DepartmentName = branch.Name ?? departmentKey,
                    CategoryId = categoryId
                }
            );
            await _coursesContext.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            return e;
        }

        return Result.FromSuccess();
    }

    /// <summary>
    /// Deassigns the given category to the department.
    /// </summary>
    /// <param name="departmentKey">The key of the department that will be deassigned.</param>
    /// <param name="ct">The cancellation token to cancel the operation.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    public async Task<Result> DeassignDepartmentCategory(string departmentKey, CancellationToken ct = default)
    {
        try
        {
            var departmentAssignment = _coursesContext.DepartmentAssignments.LastOrDefaultAsync
                (x => x.DepartmentKey == departmentKey, ct);

            _coursesContext.Remove(departmentAssignment);
            await _coursesContext.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            return e;
        }

        return Result.FromSuccess();
    }
}