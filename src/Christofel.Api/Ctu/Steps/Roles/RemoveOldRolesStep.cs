using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Christofel.Api.Ctu.Steps.Roles
{
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
                .Where(x => data.GuildUser.RoleIds.Contains(x.RoleId))
                .ToListAsync();

            data.Roles.RemoveRange(
                roleDiscordIds.Except(data.Roles.AddRoles, new RoleEqualityComparer())
            );

            return true;
        }

        private class RoleEqualityComparer : IEqualityComparer<CtuAuthRole>
        {
            public bool Equals(CtuAuthRole x, CtuAuthRole y)
            {
                return x.RoleId == y.RoleId;
            }

            public int GetHashCode(CtuAuthRole obj)
            {
                return obj.RoleId.GetHashCode();
            }
        }
    }
}