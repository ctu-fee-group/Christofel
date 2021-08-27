using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Conditions
{
    public class MemberMatchesUserCondition : IPreAuthCondition
    {
        public ValueTask<Result> CheckPreAsync(IAuthData authData, CancellationToken ct = new CancellationToken())
        {
            bool matchedUser = !authData.GuildUser.User.HasValue;

            if (matchedUser)
            {
                return ValueTask.FromResult<Result>(Result.FromSuccess());
            }

            var user = authData.GuildUser.User.Value;
            return ValueTask.FromResult<Result>(user.ID.Value == authData.DbUser.DiscordId
                ? Result.FromSuccess()
                : new InvalidOperationError("Cannot proceed with guild member ID not matching db user discord ID"));
        }
    }
}