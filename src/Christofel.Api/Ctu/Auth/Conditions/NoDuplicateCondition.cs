using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu.Resolvers;
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
        private readonly DuplicateResolver _duplicates;
        
        public NoDuplicateCondition(DuplicateResolver duplicates)
        {
            _duplicates = duplicates;
        }
        
        public async ValueTask<Result> CheckPreAsync(IAuthData authData, CancellationToken ct = new CancellationToken())
        {
            Duplicate duplicate = await _duplicates.ResolveDuplicateAsync(authData.LoadedUser, ct);
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

            return Result.FromSuccess();
        }
        

    }
}