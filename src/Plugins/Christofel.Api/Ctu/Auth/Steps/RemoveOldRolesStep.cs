using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Steps
{
    /// <summary>
    /// Softly removes discord roles that should not be added again, but were added as part of the auth process
    /// </summary>
    public class RemoveOldRolesStep : IAuthStep
    {
        public async Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default)
        {
            List<CtuAuthRole> roleDiscordIds = await data.DbContext.RoleAssignments
                .AsNoTracking()
                .Select(x => new CtuAuthRole
                {
                    RoleId = x.RoleId,
                    Type = x.RoleType
                })
                .Where(x => data.GuildUser.Roles.Contains(x.RoleId))
                .ToListAsync(ct);

            data.Roles.SoftRemoveRange(roleDiscordIds);
            
            return Result.FromSuccess();
        }
    }
}