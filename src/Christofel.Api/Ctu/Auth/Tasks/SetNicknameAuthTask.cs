using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu.JobQueue;
using Christofel.Api.Ctu.Resolvers;
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
        private readonly DuplicateResolver _duplicates;

        public SetNicknameAuthTask(IJobQueue<CtuAuthNicknameSet> jobQueue, ILogger<SetNicknameAuthTask> logger,
            DuplicateResolver duplicates)
        {
            _duplicates = duplicates;
            
            _jobQueue = jobQueue;
            _logger = logger;
        }

        public async Task<Result> ExecuteAsync(IAuthData data, CancellationToken ct = default)
        {
            var duplicate = await _duplicates.ResolveDuplicateAsync(data.LoadedUser, ct);
            if (duplicate.Type == DuplicityType.None)
            {
                await EnqueueChange(data, ct);
            }

            return Result.FromSuccess();
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

            _jobQueue.EnqueueJob(new CtuAuthNicknameSet(data.DbUser.DiscordId,
                data.GuildId, (string)nickname));
            return Task.FromResult(Result.FromSuccess());
        }
    }
}