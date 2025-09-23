//
//   ProgrammeRoleStep.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Common.Database.Models;
using Christofel.CtuAuth.Extensions;
using Kos.Abstractions;
using Kos.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.CtuAuth.Auth.Steps
{
    /// <summary>
    /// Assign roles from ProgrammeRoleAssignment table.
    /// </summary>
    /// <remarks>
    /// Uses kos api to obtain programme of the user.
    /// </remarks>
    public class ProgrammeRoleStep : IAuthStep
    {
        private readonly IKosPeopleApi _kosPeopleApi;
        private readonly IKosAtomApi _kosApi;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgrammeRoleStep"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="kosPeopleApi">The kos people api.</param>
        /// <param name="kosApi">The kos api.</param>
        public ProgrammeRoleStep
            (ILogger<ProgrammeRoleStep> logger, IKosPeopleApi kosPeopleApi, IKosAtomApi kosApi)
        {
            _kosPeopleApi = kosPeopleApi;
            _kosApi = kosApi;
            _kosApi = kosApi;
            _logger = logger;
        }

        private record ProgrammeAssignmentSpec(string programme, bool graduated, bool active);

        private async Task<Dictionary<string, ProgrammeAssignmentSpec>> GetSpecs(Person? kosPerson, CancellationToken ct = default)
        {
            var studentRoles = kosPerson?.Roles.Students;
            var activeStudents = await _kosApi.GetActiveStudentRoles(studentRoles, ct);
            var graduatedStudents = await _kosApi.GetGraduatedStudentRoles(studentRoles, ct);
            var assignmentSpecs = new Dictionary<string, ProgrammeAssignmentSpec>();

            // No active nor graduated roles,
            // fallback to latest study role.
            if (activeStudents.Count == 0 && graduatedStudents.Count == 0)
            {
                var latestStudent = await _kosApi.GetLatestStudentRole(studentRoles, ct);
                if (latestStudent?.Programme?.Title is not null)
                {
                    assignmentSpecs.Add
                        (
                            latestStudent.Programme.Title,
                            new ProgrammeAssignmentSpec(latestStudent.Programme.Title, false, true)
                        );
                }
            }

            void updateAssignmentSpecs(IReadOnlyList<Student> studentRoles, Dictionary<string, ProgrammeAssignmentSpec> assignmentSpecs, Func<ProgrammeAssignmentSpec, ProgrammeAssignmentSpec> action)
            {
                foreach (var studentRole in studentRoles)
                {
                    var programmeTitle = studentRole.Programme?.Title;

                    if (programmeTitle is null)
                    {
                        continue;
                    }

                    var assignmentSpec = assignmentSpecs.ContainsKey(programmeTitle)
                        ? assignmentSpecs[programmeTitle]
                        : new ProgrammeAssignmentSpec(programmeTitle, false, false);

                    assignmentSpec = action(assignmentSpec);

                    assignmentSpecs[programmeTitle] = assignmentSpec;
                }
            }

            updateAssignmentSpecs
                (
                    activeStudents,
                    assignmentSpecs,
                    assignmentSpec => assignmentSpec with { active = true }
                );
            updateAssignmentSpecs
                (
                    graduatedStudents,
                    assignmentSpecs,
                    assignmentSpec => assignmentSpec with { graduated = true }
                );

            return assignmentSpecs;
        }

        /// <inheritdoc />
        public async Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default)
        {
            var kosPerson =
                await _kosPeopleApi.GetPersonAsync(data.LoadedUser.CtuUsername, ct);

            var assignmentSpecs = await GetSpecs(kosPerson, ct);

            // No role assignments
            if (assignmentSpecs.Count == 0)
            {
                return Result.FromSuccess();
            }

            // NOTE: query is generated for each programme. This could possibly be avoided,
            // but generally it is expected one programmeTitle per person will be available,
            // and even if there are multiple, there aren't many users authenticating,
            // so it should be fine.
            foreach (var (programmeTitle, assignmentSpec) in assignmentSpecs)
            {
                var graduationRole = !assignmentSpec.active && assignmentSpec.graduated;

                var query = data.DbContext.ProgrammeRoleAssignments
                    .AsNoTracking()
                    .Where(x => x.Programme == programmeTitle);

                IQueryable<RoleAssignment?> finalQuery;
                if (graduationRole)
                {
                    finalQuery = query
                        .Include(x => x.GraduationAssignment)
                        .Select(x => x.GraduationAssignment);
                }
                else
                {
                    finalQuery = query
                        .Include(x => x.Assignment)
                        .Select(x => x.Assignment);
                }

                var assignments = await finalQuery
                    .ToListAsync(ct);

                if (assignments.Count == 0)
                {
                    _logger.LogWarning
                        (
                            "Could not find mapping for programme '{programmeTitle}' for user {GuildUser}",
                            programmeTitle,
                            data.GuildUser
                        );
                }

                var roles = assignments
                    .Where(x => x is not null)
                    .Select(x => new CtuAuthRole { RoleId = x!.RoleId, Type = x.RoleType });
                data.Roles.AddRange(roles);
            }

            return Result.FromSuccess();
        }
    }
}
