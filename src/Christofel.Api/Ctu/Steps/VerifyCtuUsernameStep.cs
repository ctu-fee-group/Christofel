using System.Threading.Tasks;
using Christofel.Api.Exceptions;
using Christofel.BaseLib.User;
using Microsoft.Extensions.Logging;

namespace Christofel.Api.Ctu.Steps
{
    /// <summary>
    /// Verifies that the ctu username of the user from database matches the new one trying to auth
    /// </summary>
    /// <remarks>
    /// If database username is not set, it will be after this step
    /// If it was set and does not match the same one that is now trying to authenticate,
    /// the process will result in error
    /// </remarks>
    public class VerifyCtuUsernameStep : CtuAuthStep
    {
        public VerifyCtuUsernameStep(ILogger<CtuAuthProcess> logger)
            : base(logger)
        {
        }

        protected override async Task<bool> HandleStep(CtuAuthProcessData data)
        {
            ICtuUser ctuUser = await data.CtuOauthHandler.CheckTokenAsync(
                data.AccessToken,
                data.CancellationToken
            );

            if (data.DbUser.CtuUsername != null && data.DbUser.CtuUsername != ctuUser.CtuUsername)
            {
                _logger.LogError("CTU username was set in the database, but does not match new request CTU username");
                throw new UserException(
                    "CTU username was already set in database, but does not match new request CTU username");
            }

            data.DbUser.CtuUsername = ctuUser.CtuUsername;
            return true;
        }
    }
}