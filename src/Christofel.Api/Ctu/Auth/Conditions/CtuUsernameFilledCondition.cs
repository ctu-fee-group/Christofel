using System.Threading;
using System.Threading.Tasks;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Conditions
{
    public class CtuUsernameFilledCondition : IPreAuthCondition
    {
        public ValueTask<Result> CheckPreAsync(IAuthData authData, CancellationToken ct = new CancellationToken())
        {
            return ValueTask.FromResult<Result>(string.IsNullOrEmpty(authData.LoadedUser.CtuUsername)
                ? new InvalidOperationError("Cannot proceed if ctu username is null")
                : Result.FromSuccess());
        }
    }
}