using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kos;
using Kos.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Logging;

namespace Christofel.Api.Ctu.Steps.Roles
{
    /// <summary>
    /// Assign roles from SpecificRoleAssignment table
    /// </summary>
    /// <remarks>
    /// Adds authenticated role to everyone
    /// Uses kos api to obtain whether the user is a teacher, assigns teacher role if he is
    /// </remarks>
    public class SpecificRolesStep : CtuAuthStep
    {
        private readonly KosApi _kosApi;

        public SpecificRolesStep(ILogger<CtuAuthProcess> logger, KosApi kosApi)
            : base(logger)
        {
            _kosApi = kosApi;
        }

        protected override async Task<bool> HandleStep(CtuAuthProcessData data)
        {
            List<string> assignRoleNames = new List<string>();
            assignRoleNames.Add("Authentication");

            // Check if is teacher
            if (await IsTeacherAsync(
                data.DbUser.CtuUsername ?? throw new InvalidOperationException("CtuUsername is null"), data.AccessToken,
                data.CancellationToken))
            {
                assignRoleNames.Add("Teacher");
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
                .ToListAsync();

            if (assignRoleNames.Count > assignRoleIds.Count)
            {
                IEnumerable<string?> notFoundRoleNames = assignRoleNames.Except(assignRoleIds.Select(x => x.Description));

                foreach (string? notFoundRole in notFoundRoleNames)
                {
                    _logger.LogWarning($"Could not obtain specific role {notFoundRole ?? "Unknown"} from database.");

                    if (notFoundRole == "Authentication")
                    {
                        throw new InvalidOperationException("Could not find mandatory Authentication role mapping");
                    }
                }
            }

            data.Roles.AddRange(assignRoleIds);

            return true;
        }

        private async Task<bool> IsTeacherAsync(string username, string accessToken, CancellationToken token = default)
        {
            KosPerson? person = await _kosApi.GetAuthorizedApi(accessToken)
                .People.GetPerson(username, token);

            return person?.Roles?.Teacher != null;
        }
    }
}