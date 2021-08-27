using System.Threading;
using System.Threading.Tasks;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Conditions
{
    public class CtuUsernameMatchesCondition : IPreAuthCondition
    {
        public ValueTask<Result> CheckPreAsync(IAuthData authData, CancellationToken ct = new CancellationToken())
        {
            if (authData.DbUser.CtuUsername is null)
            {
                return ValueTask.FromResult<Result>(Result.FromSuccess());
            }

            return ValueTask.FromResult<Result>(authData.LoadedUser.CtuUsername == authData.DbUser.CtuUsername
                ? Result.FromSuccess()
                : new InvalidOperationError("CtuUsername in the database does not match the loaded one"));
        }
    }
}