using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu.JobQueue;
using HotChocolate.Execution.Processing;
using Microsoft.Extensions.Logging;
using Remora.Discord.Core;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Tasks
{
    public class SetNicknameAuthTask : IAuthTask
    {
        private readonly IJobQueue<CtuAuthNicknameSet> _jobQueue;
        private readonly ILogger _logger;

        public SetNicknameAuthTask(IJobQueue<CtuAuthNicknameSet> jobQueue, ILogger<SetNicknameAuthTask> logger)
        {
            _jobQueue = jobQueue;
            _logger = logger;
        }

        public Task<Result> ExecuteAsync(IAuthData data, CancellationToken ct = default)
        {
            if (!data.StepData.TryGetValue("Duplicate", out var duplicateObj) || duplicateObj is null)
            {
                return Task.FromResult<Result>(new InvalidOperationError(
                    "Could not find duplicate in step data. Did you forget to register duplicate condition?"));
            }

            var duplicate = (Duplicate)duplicateObj;
            if (duplicate.Type == DuplicityType.None)
            {
                // First registration, change nickname
            }

            return Task.FromResult(Result.FromSuccess());
        }

        private Task<Result> EnqueueChange(IAuthData data, CancellationToken ct)
        {
            if (!data.StepData.TryGetValue("Nickname", out var nickname))
            {
                return Task.FromResult<Result>(new InvalidOperationError(
                    "Could not find nickname in step data. Did you forget to register set nickname step?"));
            }

            if (nickname is null)
            {
                _logger.LogWarning("Could not obtain what the user's nickname should be");
                return Task.FromResult(Result.FromSuccess());
            }

            _jobQueue.EnqueueJob(new CtuAuthNicknameSet(new Snowflake(data.DbUser.DiscordId),
                new Snowflake(data.GuildId), (string)nickname));
            return Task.FromResult(Result.FromSuccess());
        }
    }
}