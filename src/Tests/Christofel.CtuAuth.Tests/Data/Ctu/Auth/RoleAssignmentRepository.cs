//
//   RoleAssignmentRepository.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Christofel.Common.Database;
using Christofel.Common.Database.Models;
using Christofel.Common.Database.Models.Enums;
using Christofel.CtuAuth.Tests.Data.Ctu.Auth.KosSource;
using Kos.Data;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;

namespace Christofel.CtuAuth.Tests.Data.Ctu.Auth;

/// <summary>
/// A repository with common role assignments.
/// </summary>
public class RoleAssignmentRepository
{
    /// <summary>
    /// Gets the year-based role assignments.
    /// </summary>
    public IReadOnlyDictionary<int, Snowflake> YearRoles { get; }

    /// <summary>
    /// Gets the programme-based role assignments.
    /// </summary>
    public IReadOnlyDictionary<string, Snowflake> ProgrammeRoles { get; }

    /// <summary>
    /// Gets the graduation programme-based role assignments.
    /// </summary>
    public IReadOnlyDictionary<string, Snowflake> GraduationProgrammeRoles { get; }

    /// <summary>
    /// Gets the usermap role assignments.
    /// </summary>
    public IReadOnlyDictionary<string, Snowflake> UsermapRoles { get; }

    /// <summary>
    /// Gets the username-specific role assignments.
    /// </summary>
    public IReadOnlyDictionary<string, Snowflake> UsernameRoles { get; }

    /// <summary>
    /// Gets the programme type-based role assignments.
    /// </summary>
    public IReadOnlyDictionary<ProgrammeType, Snowflake> ProgrammeTypeRoles { get; }

    /// <summary>
    /// Gets the pre-title role assignments.
    /// </summary>
    public IReadOnlyDictionary<string, Snowflake> PreTitleRoles { get; }

    /// <summary>
    /// Gets the pre-title role priorities.
    /// </summary>
    public IReadOnlyDictionary<string, uint> PreTitleRolesPriorities { get; }

    /// <summary>
    /// Gets the post-title role assignments.
    /// </summary>
    public IReadOnlyDictionary<string, Snowflake> PostTitleRoles { get; }

    /// <summary>
    /// Gets the teacher role.
    /// </summary>
    public Snowflake TeacherRole { get; }

    /// <summary>
    /// Gets the authenticated role.
    /// </summary>
    public Snowflake AuthenticatedRole { get; }

    private ulong _next = 0;

    private Snowflake nextSnowflake()
    {
        return new Snowflake(_next++, 0);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleAssignmentRepository"/> class.
    /// </summary>
    public RoleAssignmentRepository()
    {
        AuthenticatedRole = nextSnowflake();
        TeacherRole = nextSnowflake();

        var bachelor = nextSnowflake();
        var master = nextSnowflake();
        var doctoral = nextSnowflake();

        YearRoles = new Dictionary<int, Snowflake>()
        {
            { 2020, nextSnowflake() },
            { 2021, nextSnowflake() },
            { 2022, nextSnowflake() },
            { 2023, nextSnowflake() },
            { 2024, nextSnowflake() },
            { 2025, nextSnowflake() },
        }.AsReadOnly();

        ProgrammeRoles = new Dictionary<string, Snowflake>()
        {
            { "Programme1", nextSnowflake() },
            { "Programme2", nextSnowflake() },
            { "Programme3", nextSnowflake() },
            { "Programme4", nextSnowflake() },
        }.AsReadOnly();

        GraduationProgrammeRoles = new Dictionary<string, Snowflake>()
        {
            { "Programme1", nextSnowflake() },
            { "Programme2", nextSnowflake() },
            { "Programme3", nextSnowflake() },
            { "Programme4", nextSnowflake() },
        }.AsReadOnly();

        UsermapRoles = new Dictionary<string, Snowflake>()
        {
            { "A_USERMAP_ROLE", nextSnowflake() },
            { "ANOTHER_UMAPI_ROLE", nextSnowflake() },
            { "BACHELOR", bachelor },
            { "MASTER", master },
            { "DOCTORAL", doctoral },
        }.AsReadOnly();

        UsernameRoles = new Dictionary<string, Snowflake>()
        {
            { "user1", nextSnowflake() },
            { "user2", nextSnowflake() },
        }.AsReadOnly();

        ProgrammeTypeRoles = new Dictionary<ProgrammeType, Snowflake>()
        {
            { ProgrammeType.Bachelor, bachelor },
            { ProgrammeType.Master, master },
            { ProgrammeType.Doctoral, doctoral },
        }.AsReadOnly();

        PreTitleRoles = new Dictionary<string, Snowflake>()
        {
            { "prof.", nextSnowflake() },
            { "doc.", nextSnowflake() },
            { "Mgr.", nextSnowflake() },
            { "Ing.", nextSnowflake() },
            { "Bc.", nextSnowflake() },
        }.AsReadOnly();

        PreTitleRolesPriorities = new Dictionary<string, uint>()
        {
            { "prof.", 10 },
            { "doc.", 9 },
            { "Mgr.", 2 },
            { "Ing.", 2 },
            { "Bc.", 1 },
        }.AsReadOnly();

        PostTitleRoles = new Dictionary<string, Snowflake>()
        {
            { "Ph.D.", nextSnowflake() }
        }.AsReadOnly();
    }

    /// <summary>
    /// Prepares a role assignment for the given role.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="role">The role snowflake.</param>
    /// <returns>The role assignment.</returns>
    private async Task<RoleAssignment> PrepareRoleAssignment(ChristofelBaseContext dbContext, Snowflake role)
    {
        var roleAssignment = await dbContext.RoleAssignments
            .AsQueryable()
            .Where(x => x.RoleId == role)
            .FirstOrDefaultAsync();

        if (roleAssignment is null)
        {
            roleAssignment = new RoleAssignment()
            {
                RoleId = role,
                RoleType = RoleType.General
            };
            dbContext.Add(roleAssignment);
        }

        return roleAssignment;
    }

    /// <summary>
    /// Fills the database with test role assignments.
    /// </summary>
    /// <param name="dbContext">The database context to fill.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task FillDatabase(ChristofelBaseContext dbContext)
    {
        { // Authenticated
            var roleAssignment = await PrepareRoleAssignment(dbContext, AuthenticatedRole);
            dbContext.Add(new SpecificRoleAssignment
            {
                Name = "Authentication",
                Assignment = roleAssignment,
            });
        }

        { // Teacher
            var roleAssignment = await PrepareRoleAssignment(dbContext, TeacherRole);
            dbContext.Add(new SpecificRoleAssignment
            {
                Name = "Teacher",
                Assignment = roleAssignment,
            });
        }

        foreach (var (year, role) in YearRoles)
        {
            var roleAssignment = await PrepareRoleAssignment(dbContext, role);
            dbContext.Add(new YearRoleAssignment()
            {
                Year = year,
                Assignment = roleAssignment
            });
        }

        foreach (var (programme, role) in ProgrammeRoles)
        {
            var roleAssignment = await PrepareRoleAssignment(dbContext, role);
            var gradRoleAssignment = await PrepareRoleAssignment(dbContext, GraduationProgrammeRoles[programme]);
            dbContext.Add(new ProgrammeRoleAssignment()
            {
                Programme = programme,
                GraduationAssignment = gradRoleAssignment,
                Assignment = roleAssignment
            });
        }

        foreach (var (usermapRole, role) in UsermapRoles)
        {
            var roleAssignment = await PrepareRoleAssignment(dbContext, role);
            dbContext.Add(new UsermapRoleAssignment
            {
                UsermapRole = usermapRole,
                Assignment = roleAssignment
            });
        }

        foreach (var (username, role) in UsernameRoles)
        {
            var roleAssignment = await PrepareRoleAssignment(dbContext, role);
            dbContext.Add(new UsernameRoleAssignment
            {
                Username = username,
                Assignment = roleAssignment
            });
        }

        foreach (var (programmeType, role) in ProgrammeTypeRoles)
        {
            var roleAssignment = await PrepareRoleAssignment(dbContext, role);
            dbContext.Add(new SpecificRoleAssignment()
            {
                Name = $"{programmeType.ToString()}Programme",
                Assignment = roleAssignment
            });
        }

        foreach (var (preTitle, role) in PreTitleRoles)
        {
            var priority = PreTitleRolesPriorities[preTitle];
            var roleAssignment = await PrepareRoleAssignment(dbContext, role);
            dbContext.Add(new TitleRoleAssignment
            {
                Title = preTitle,
                Pre = true,
                Post = false,
                Priority = priority,
                Assignment = roleAssignment
            });
        }

        foreach (var (postTitle, role) in PostTitleRoles)
        {
            var roleAssignment = await PrepareRoleAssignment(dbContext, role);
            dbContext.Add(new TitleRoleAssignment
            {
                Title = postTitle,
                Pre = false,
                Post = true,
                Priority = 10,
                Assignment = roleAssignment
            });
        }

        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Computes the roles that should be assigned based on the provided information.
    /// </summary>
    /// <param name="roleAssignmentInfo">The role assignment information.</param>
    /// <returns>The roles that should be assigned.</returns>
    public IEnumerable<Snowflake> ComputeRolesFor(RoleAssignmentInfo roleAssignmentInfo)
    {
#pragma warning disable SA1010
        yield return AuthenticatedRole;

        if (UsernameRoles.ContainsKey(roleAssignmentInfo.Username))
        {
            yield return UsernameRoles[roleAssignmentInfo.Username];
        }

        if (roleAssignmentInfo.Teacher)
        {
            yield return TeacherRole;
        }

        foreach (var year in roleAssignmentInfo.Years ?? [])
        {
            yield return YearRoles[year];
        }

        foreach (var programmeType in roleAssignmentInfo.ProgrammeTypes ?? [])
        {
            yield return ProgrammeTypeRoles[programmeType];
        }

        foreach (var usermapRole in roleAssignmentInfo.UsermapRoles ?? [])
        {
            yield return UsermapRoles[usermapRole];
        }

        foreach (var preTitle in roleAssignmentInfo.PreTitles?.ToArray() ?? [])
        {
            yield return PreTitleRoles[preTitle];
        }

        foreach (var postTitle in roleAssignmentInfo.PostTitles ?? [])
        {
            yield return PostTitleRoles[postTitle];
        }

        foreach (var activeProgramme in roleAssignmentInfo.ActiveProgrammes ?? [])
        {
            yield return ProgrammeRoles[activeProgramme];
        }

        foreach (var graduatedProgramme in roleAssignmentInfo.GraduatedProgrammes ?? [])
        {
            yield return GraduationProgrammeRoles[graduatedProgramme];
        }
#pragma warning restore SA1010
    }
}
