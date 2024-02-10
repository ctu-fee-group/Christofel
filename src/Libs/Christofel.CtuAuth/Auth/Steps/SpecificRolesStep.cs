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
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecificRolesStep"/> class.
        /// </summary>
        /// <param name="kosPeopleApi">The kos people api.</param>
        /// <param name="kosApi">The kos api.</param>
        /// <param name="logger">The logger.</param>
        public SpecificRolesStep(IKosPeopleApi kosPeopleApi, IKosAtomApi kosApi, ILogger<SpecificRolesStep> logger)
        {
            _logger = logger;
            _kosPeopleApi = kosPeopleApi;
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

            var currentStudiesRole =
                await ObtainCurrentStudies(data.LoadedUser.CtuUsername, ct);

            if (currentStudiesRole is not null)
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
                    _logger.LogWarning($"Could not obtain specific role {notFoundRole ?? "Unknown"} from database.");

                    if (notFoundRole == "Authentication")
                    {
                        return new InvalidOperationError("Could not find mandatory Authentication role mapping");
                    }
                }
            }

            data.Roles.AddRange(assignRoleIds);
            return Result.FromSuccess();
        }

        private async Task<string?> ObtainCurrentStudies
        (
            string username,
            CancellationToken token = default
        )
        {
            var person = await _kosPeopleApi.GetPersonAsync(username, token);
            var student = await _kosApi.GetLatestStudentRole(person?.Roles.Students, ct: token);

            if (student is null)
            {
                return null;
            }

            try
            {
                var programme = await _kosApi.LoadEntryAsync(student.Programme, token: token);

                if (programme is null)
                {
                    return null;
                }

                return programme.Content.ProgrammeType switch
                {
                    ProgrammeType.Bachelor => "BachelorProgramme",
                    ProgrammeType.Master => "MasterProgramme",
                    ProgrammeType.MasterLegacy => "MasterProgramme",
                    ProgrammeType.Doctoral => "DoctoralProgramme",
                    _ => null,
                };
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "There was an exception thrown whilst obtaining a programme.");
            }

            return null;
        }

        private async Task<bool> IsTeacherAsync(string username, CancellationToken token = default)
        {
            var person = await _kosPeopleApi.GetPersonAsync(username, token);
            return person?.Roles?.Teacher != null;
        }
    }
}