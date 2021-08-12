using System;
using System.Threading.Tasks;
using Christofel.Api.Exceptions;
using Christofel.BaseLib.User;
using Microsoft.Extensions.Logging;

namespace Christofel.Api.Ctu.Steps
{
    public class VerifyCtuUsernameStep : CtuAuthStep
    {
        public VerifyCtuUsernameStep(ILogger<VerifyCtuUsernameStep> logger)
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