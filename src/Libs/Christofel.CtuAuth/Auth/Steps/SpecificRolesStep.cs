//
//   SpecificRolesStep.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.CtuAuth.Extensions;
using Kos.Abstractions;
using Kos.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.CtuAuth.Auth.Steps
{
    /// <summary>
    /// Assign roles from SpecificRoleAssignment table.
    /// </summary>
    /// <remarks>
    /// Adds authenticated role to everyone.
    /// Uses kos api to obtain whether the user is a teacher, assigns teacher role if he is.
    /// Uses kos api to obtain current studies of the student.
    /// </remarks>
    public class SpecificRolesStep : IAuthStep
    {
        private readonly IKosAtomApi _kosApi;
        private readonly IKosPeopleApi _kosPeopleApi;
        private readonly IKosProgrammesApi _kosProgrammesApi;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecificRolesStep"/> class.
        /// </summary>
        /// <param name="kosPeopleApi">The kos people api.</param>
        /// <param name="kosProgrammesApi">The kos programmes api.</param>
        /// <param name="kosApi">The kos api.</param>
        /// <param name="logger">The logger.</param>
        public SpecificRolesStep
        (
            IKosPeopleApi kosPeopleApi,
            IKosProgrammesApi kosProgrammesApi,
            IKosAtomApi kosApi,
            ILogger<SpecificRolesStep> logger
        )
        {
            _logger = logger;
            _kosPeopleApi = kosPeopleApi;
            _kosProgrammesApi = kosProgrammesApi;
            _kosApi = kosApi;
        }

        /// <inheritdoc />
        public async Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default)
        {
            List<string> assignRoleNames = new List<string>();
            assignRoleNames.Add("Authentication");

            // Check if is teacher
            if (await IsTeacherAsync
            (
                data.LoadedUser.CtuUsername,
                ct
            ))
            {
                assignRoleNames.Add("Teacher");
            }

            var currentStudiesRoles =
                await ObtainCurrentStudies(data.LoadedUser.CtuUsername, ct);

            foreach (var currentStudiesRole in currentStudiesRoles)
            {
                assignRoleNames.Add(currentStudiesRole);
            }

            List<CtuAuthRole> assignRoleIds = await data.DbContext.SpecificRoleAssignments
                .AsNoTracking()
                .Where(x => assignRoleNames.Contains(x.Name))
                .Include(x => x.Assignment)
                .Select
                (
                    x => new CtuAuthRole
                    {
                        RoleId = x.Assignment.RoleId, Type = x.Assignment.RoleType, Description = x.Name,
                    }
                )
                .ToListAsync(ct);

            if (assignRoleNames.Count > assignRoleIds.Count)
            {
                IEnumerable<string?> notFoundRoleNames =
                    assignRoleNames.Except(assignRoleIds.Select(x => x.Description));

                foreach (var notFoundRole in notFoundRoleNames)
                {
                    _logger.LogWarning("Could not obtain specific role {Role} from database.", notFoundRole ?? "Unknown");

                    if (notFoundRole == "Authentication")
                    {
                        return new InvalidOperationError("Could not find mandatory Authentication role mapping");
                    }
                }
            }

            data.Roles.AddRange(assignRoleIds);
            return Result.FromSuccess();
        }

        // Obtains ProgrammeType of all active student roles.
        private async Task<IReadOnlyList<string>> ObtainCurrentStudies
        (
            string username,
            CancellationToken token = default
        )
        {
            var person = await _kosPeopleApi.GetPersonAsync(username, token);
            var studentRoles = await _kosApi
                .GetActiveStudentRoles(person?.Roles.Students, ct: token);

            var programTypes = new List<string>();
            foreach (var student in studentRoles)
            {
                try
                {
                    if (student.Programme is null)
                    {
                        _logger.LogWarning($"Student role doesn't have a programme, skipping programme type role assignment.");
                        continue;
                    }

                    var (programme, unique) = await _kosProgrammesApi
                        .GetNonUniqueProgramme(student.Programme, ct: token);

                    if (!unique)
                    {
                        _logger.LogWarning($"Programme {student.Programme?.Title} ({student.Programme!.GetKey()}) is not unique, assuming first one is the correct one.");
                    }

                    if (programme is null)
                    {
                        _logger.LogWarning($"Programme {student.Programme?.Title} ({student.Programme!.GetKey()}) not found, cannot assign programme type role.");
                        continue;
                    }

                    var programType = programme.ProgrammeType switch
                    {
                        ProgrammeType.Bachelor => "BachelorProgramme",
                        ProgrammeType.Master => "MasterProgramme",
                        ProgrammeType.MasterLegacy => "MasterProgramme",
                        ProgrammeType.Doctoral => "DoctoralProgramme",
                        _ => null,
                    };

                    if (programType is not null)
                    {
                        programTypes.Add(programType);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogWarning
                        (
                            e,
                            "There was an exception thrown whilst obtaining a programme."
                        );
                }
            }

            return programTypes.AsReadOnly();
        }

        private async Task<bool> IsTeacherAsync(string username, CancellationToken token = default)
        {
            var person = await _kosPeopleApi.GetPersonAsync(username, token);
            return person?.Roles?.Teacher != null;
        }
    }
}
