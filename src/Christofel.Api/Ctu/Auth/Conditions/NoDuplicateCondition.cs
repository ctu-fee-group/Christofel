using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.GraphQL.Common;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Extensions;
using Microsoft.EntityFrameworkCore;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Conditions
{
    public class NoDuplicateCondition : IPreAuthCondition
    {
        public async ValueTask<Result> CheckPreAsync(IAuthData authData, CancellationToken ct = new CancellationToken())
        {
            Duplicate duplicate = await GetDuplicityAsync(authData, ct);
            DuplicityType duplicityType = duplicate.Type;

            switch (duplicityType)
            {
                case DuplicityType.CtuSide:
                case DuplicityType.DiscordSide:
                    if (!authData.DbUser.DuplicityApproved)
                    {
                        return UserErrors.RejectedDuplicateUser;
                    }

                    break;
            }

            authData.StepData.Add("Duplicate", duplicate);
            return Result.FromSuccess();
        }
        
        private async Task<Duplicate> GetDuplicityAsync(IAuthData data, CancellationToken ct = default)
        {
            ChristofelBaseContext dbContext = data.DbContext;
            DbUser dbUser = data.DbUser;

            DbUser? duplicateBoth = await dbContext.Users
                .AsQueryable()
                .Authenticated()
                .Where(x => x.CtuUsername == data.LoadedUser.CtuUsername && x.DiscordId == dbUser.DiscordId)
                .FirstOrDefaultAsync(ct);

            if (duplicateBoth != null)
            {
                return new Duplicate(DuplicityType.Both, duplicateBoth);
            }

            DbUser? duplicateDiscord = await dbContext.Users
                .AsQueryable()
                .Authenticated()
                .Where(x => x.DiscordId == dbUser.DiscordId)
                .FirstOrDefaultAsync(ct);

            if (duplicateDiscord != null)
            {
                return new Duplicate(DuplicityType.DiscordSide, duplicateDiscord);
            }

            DbUser? duplicateCtu = await dbContext.Users
                .AsQueryable()
                .Authenticated()
                .Where(x => x.CtuUsername == data.LoadedUser.CtuUsername)
                .FirstOrDefaultAsync(ct);

            if (duplicateCtu != null)
            {
                return new Duplicate(DuplicityType.CtuSide, duplicateCtu);
            }

            return new Duplicate(DuplicityType.None, dbUser);
        }
    }
}