using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.OAuth;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Microsoft.Extensions.Logging;

namespace Christofel.Api.Ctu
{
    /// <summary>
    /// Used to iterate through all steps of auth process
    /// </summary>
    public class CtuAuthProcess
    {
        private readonly CtuAuthStepProvider _stepProvider;
        private readonly ILogger<CtuAuthProcess> _logger;

        /// <summary>
        /// Initialize CtuAuthProcess
        /// </summary>
        /// <param name="stepProvider">Provider of CtuAuthStep to get all needed steps</param>
        /// <param name="logger">Logger to log with</param>
        public CtuAuthProcess(CtuAuthStepProvider stepProvider, ILogger<CtuAuthProcess> logger)
        {
            _logger = logger;
            _stepProvider = stepProvider;
        }

        /// <summary>
        /// Proceed to do all the steps
        /// If step fails, exception will be thrown
        /// </summary>
        /// <param name="accessToken">Valid token that can be used for Kos and Usermap</param>
        /// <param name="ctuOauthHandler"></param>
        /// <param name="dbContext">Context monitoring dbUser</param>
        /// <param name="dbUser">Database user to be edited and saved</param>
        /// <param name="guildUser">Discord user used for auth purposes. Should be user with the id of dbUser</param>
        /// <param name="token">Cancellation token in case the request is cancelled</param>
        public async Task FinishAuthAsync(string accessToken, CtuOauthHandler ctuOauthHandler,
            ChristofelBaseContext dbContext, DbUser dbUser,
            RestGuildUser guildUser, CancellationToken token = default)
        {
            IEnumerable<ICtuAuthStep> steps = _stepProvider.GetSteps();
            using IEnumerator<ICtuAuthStep> stepsEnumerator = steps.GetEnumerator();

            CtuAuthProcessData data = new CtuAuthProcessData(
                accessToken,
                ctuOauthHandler,
                dbContext,
                dbUser,
                guildUser,
                new CtuAuthAssignedRoles(),
                token
            );

            try
            {
                if (stepsEnumerator.MoveNext())
                {
                    await stepsEnumerator.Current.Handle(data, GetNextHandler(stepsEnumerator));
                }
            }
            finally
            {
                try
                {
                    await dbContext.SaveChangesAsync(token);

                    if (data.Finished)
                    {
                        _logger.LogInformation("User was successfully authenticated");
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e,
                        $"database context save changes has thrown an exception while saving user data ({data.DbUser.UserId} {data.DbUser.DiscordId} {data.DbUser.CtuUsername})");
                }
            }
        }

        private Func<CtuAuthProcessData, Task> GetNextHandler(IEnumerator<ICtuAuthStep> stepsEnumerator)
        {
            bool used = false;
            return data =>
            {
                if (used)
                {
                    throw new InvalidOperationException("Next was already called");
                }
                used = true;
                
                if (stepsEnumerator.MoveNext())
                {
                    return stepsEnumerator.Current.Handle(data, GetNextHandler(stepsEnumerator));
                }

                return Task.CompletedTask;
            };
        }
    }
}