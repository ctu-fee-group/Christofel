using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.OAuth;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.User;
using Discord.Rest;

namespace Christofel.Api.Ctu
{
    public class CtuAuthProcess
    {
        private readonly CtuAuthStepProvider _stepProvider;

        public CtuAuthProcess(CtuAuthStepProvider stepProvider)
        {
            _stepProvider = stepProvider;
        }

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
                await dbContext.SaveChangesAsync(token);
            }
        }
        private Func<CtuAuthProcessData, Task> GetNextHandler(IEnumerator<ICtuAuthStep> stepsEnumerator)
        {
            return data =>
            {
                if (stepsEnumerator.MoveNext())
                {
                    return stepsEnumerator.Current.Handle(data, GetNextHandler(stepsEnumerator));
                }

                return Task.CompletedTask;
            };
        }
    }
}