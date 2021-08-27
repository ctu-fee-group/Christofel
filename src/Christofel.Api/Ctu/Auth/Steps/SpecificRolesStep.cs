using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kos;
using Kos.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Steps
{
    /// <summary>
    /// Assign roles from SpecificRoleAssignment table
    /// </summary>
    /// <remarks>
    /// Adds authenticated role to everyone
    /// Uses kos api to obtain whether the user is a teacher, assigns teacher role if he is
    /// </remarks>
    public class SpecificRolesStep : IAuthStep
    {
        private readonly AuthorizedKosApi _kosApi;
        private readonly ILogger _logger;

        public SpecificRolesStep(AuthorizedKosApi kosApi, ILogger<SpecificRolesStep> logger)
        {
            _logger = logger;
            _kosApi = kosApi;
        }
        
        public async Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default)
        {
            List<string> assignRoleNames = new List<string>();
            assignRoleNames.Add("Authentication");

            // Check if is teacher
            if (await IsTeacherAsync(
                data.LoadedUser.CtuUsername,
                ct))
            {
                assignRoleNames.Add("Teacher");
            }

            string? currentStudiesRole =
                await ObtainCurrentStudies(data.LoadedUser.CtuUsername, ct);

            if (currentStudiesRole is not null)
            {
                assignRoleNames.Add(currentStudiesRole);
            }

            List<CtuAuthRole> assignRoleIds = await data.DbContext.SpecificRoleAssignments
                .AsNoTracking()
                .Where(x => assignRoleNames.Contains(x.Name))
                .Include(x => x.Assignment)
                .Select(x => new CtuAuthRole
                {
                    RoleId = x.Assignment.RoleId,
                    Type = x.Assignment.RoleType,
                    Description = x.Name
                })
                .ToListAsync(ct);

            if (assignRoleNames.Count > assignRoleIds.Count)
            {
                IEnumerable<string?> notFoundRoleNames =
                    assignRoleNames.Except(assignRoleIds.Select(x => x.Description));

                foreach (string? notFoundRole in notFoundRoleNames)
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
        
        private async Task<string?> ObtainCurrentStudies(string username,
            CancellationToken token = default)
        {
            KosPerson? person = await _kosApi.People.GetPersonAsync(username, token: token);
            KosStudent? student = await _kosApi
                .LoadEntityAsync(person?.Roles?.Students?.LastOrDefault(), token: token);

            if (student is null)
            {
                return null;
            }

            KosProgramme? programme = await _kosApi.LoadEntityAsync(student.Programme, token: token);

            if (programme is null)
            {
                return null;
            }

            return programme.ProgrammeType switch
            {
                KosProgrammeType.Bachelor => "BachelorProgramme",
                KosProgrammeType.Master => "MasterProgramme",
                KosProgrammeType.MasterLegacy => "MasterProgramme",
                KosProgrammeType.Doctoral => "DoctoralProgramme",
                _ => null
            };
        }

        private async Task<bool> IsTeacherAsync(string username, CancellationToken token = default)
        {
            KosPerson? person = await _kosApi.People.GetPersonAsync(username, token: token);
            return person?.Roles?.Teacher != null;
        }
    }
}