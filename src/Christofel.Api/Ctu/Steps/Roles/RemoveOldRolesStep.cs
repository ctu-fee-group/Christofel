using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remora.Discord.Core;

namespace Christofel.Api.Ctu.Steps.Roles
{
    /// <summary>
    /// Softly removes discord roles that should not be added again, but were added as part of the auth process
    /// </summary>
    public class RemoveOldRolesStep : CtuAuthStep
    {
        public RemoveOldRolesStep(ILogger<CtuAuthProcess> logger) : base(logger)
        {
        }

        protected override async Task<bool> HandleStep(CtuAuthProcessData data)
        {
            List<CtuAuthRole> roleDiscordIds = await data.DbContext.RoleAssignments
                .AsNoTracking()
                .Select(x => new CtuAuthRole
                {
                    RoleId = x.RoleId,
                    Type = x.RoleType
                })
                .Where(x => data.GuildUser.Roles.Select(r => r.Value).Contains(x.RoleId))
                .ToListAsync();

            data.Roles.RemoveRange(
                roleDiscordIds.Except(data.Roles.AddRoles, new RoleEqualityComparer())
            );

            return true;
        }

        private class RoleEqualityComparer : IEqualityComparer<CtuAuthRole>
        {
            public bool Equals(CtuAuthRole? x, CtuAuthRole? y)
            {
                return x?.RoleId == y?.RoleId;
            }

            public int GetHashCode(CtuAuthRole obj)
            {
                return obj.RoleId.GetHashCode();
            }
        }
    }
}