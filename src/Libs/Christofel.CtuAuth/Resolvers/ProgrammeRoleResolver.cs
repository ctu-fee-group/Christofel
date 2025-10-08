//
//   ProgrammeRoleResolver.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Common.Database;
using Christofel.Common.Database.Models;
using Kos.Abstractions;
using Kos.Data;
using Microsoft.EntityFrameworkCore;

namespace Christofel.CtuAuth.Resolvers;

/// <summary>
/// Resolves programme assignments for kos students.
/// </summary>
public class ProgrammeRoleResolver
{
    private readonly IKosPeopleApi _kosPeopleApi;
    private readonly IKosAtomApi _kosApi;
    private readonly IReadableDbContext<ChristofelBaseContext> _baseContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgrammeRoleResolver"/> class.
    /// </summary>
    /// <param name="kosPeopleApi">The kos people api.</param>
    /// <param name="kosApi">The kos api.</param>
    /// <param name="baseContext">The christofel database context.</param>
    public ProgrammeRoleResolver(IKosPeopleApi kosPeopleApi, IKosAtomApi kosApi, IReadableDbContext<ChristofelBaseContext> baseContext)
    {
        _kosPeopleApi = kosPeopleApi;
        _kosApi = kosApi;
        _baseContext = baseContext;
    }

    /// <summary>
    /// Gets a spec and a role assignment for given kos student.
    /// </summary>
    /// <param name="student">The student to obtain programme from.</param>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <returns>The spec for given student and its role assignment.</returns>
    public async Task<(ProgrammeAssignmentSpec, ProgrammeRoleAssignment)?> GetAssignmentAsync(Student student, CancellationToken ct = default)
    {
        if (student.Programme is null || student.Programme.Title is null)
        {
            return null;
        }

        var assignment = await _baseContext.Set<ProgrammeRoleAssignment>()
            .Where(x => x.Programme == student.Programme.Title)
            .FirstOrDefaultAsync(ct);

        if (assignment is null)
        {
            return null;
        }

        var spec = new ProgrammeAssignmentSpec(
            student.Programme.Title,
            student.StudyTerminationReason == StudyTermination.Graduation,
            student.StudyState != StudyState.Closed && (student.EndDate is null || student.EndDate > DateTime.Now)
        );

        return (spec, assignment);
    }

    /// <summary>
    /// Answers if the given <see ref="student"/> should lead to an assignment of a role on the Discord guild.
    /// </summary>
    /// <param name="student">The student to check the programme for.</param>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <returns>Whether the <see ref="student"/> leads to assignment of a programme.</returns>
    public async Task<bool> AssignsProgrammeRoleAsync(Student student, CancellationToken ct = default)
    {
        var assignment = await GetAssignmentAsync(student, ct);

        if (assignment is null)
        {
            return false;
        }

        var (spec, assign) = assignment.Value;

        return (spec.Active && assign.AssignmentId is not null) ||
               (spec.Graduated && assign.GraduationAssignmentId is not null);
    }
}

/// <summary>
/// A spec built from a KOS Programme for a given Student role.
/// Specifying the programme the student is studying, whether graduated
/// or whether actively studying.
/// </summary>
/// <param name="Programme">The programme name.</param>
/// <param name="Graduated">Whether the student has graduated this programme.</param>
/// <param name="Active">Whether the student is actively studying this programme.</param>
public record ProgrammeAssignmentSpec(string Programme, bool Graduated, bool Active);
