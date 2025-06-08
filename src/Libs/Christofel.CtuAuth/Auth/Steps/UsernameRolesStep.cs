//
//   UsernameRolesStep.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Christofel.CtuAuth;
using Christofel.CtuAuth.Auth.Steps;
using Microsoft.EntityFrameworkCore;
using Remora.Results;

/// <summary>
/// Authentication step to add roles according to UsernameRoleAssignment.
/// Checks username of the user, if matches, assigns the role.
/// </summary>
public class UsernameRolesStep : IAuthStep
{
    // private readonly ILogger<UsernameRolesStep> _logger;

    // public UsernameRolesStep(ILogger<UsernameRolesStep> logger)
    // {
    //     _logger = logger;
    // }

    /// <inheritdoc/>
    public async Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default)
    {
        var assignRoleIds = await data.DbContext.UsernameRoleAssignment
            .AsNoTracking()
            .Where(x => x.Username == data.LoadedUser.CtuUsername)
            .Select(x => new CtuAuthRole
            {
                RoleId = x.Assignment.RoleId,
                Type = x.Assignment.RoleType,
                Description = "Assignment by username"
            })
            .ToListAsync(ct);

        data.Roles.AddRange(assignRoleIds);
        return Result.FromSuccess();
    }
}
