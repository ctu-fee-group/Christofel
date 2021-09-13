//
//   SetNicknameAuthTask.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu.Jobs;
using Christofel.Api.Ctu.Resolvers;
using Christofel.Scheduler;
using Christofel.Scheduler.Abstractions;
using Christofel.Scheduler.Triggers;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Tasks
{
    /// <summary>
    /// Task for assigning nickname to the user.
    /// </summary>
    /// <remarks>
    /// Assigns nickname to newly registered users only.
    /// </remarks>
    public class SetNicknameAuthTask : IAuthTask
    {
        private readonly DuplicateResolver _duplicates;
        private readonly IScheduler _scheduler;
        private readonly NonConcurrentTrigger.State _ncState;
        private readonly ILogger _logger;
        private readonly NicknameResolver _nickname;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetNicknameAuthTask"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="duplicates">The duplicate resolver.</param>
        /// <param name="nickname">The nickname resolver.</param>
        /// <param name="scheduler">The scheduler.</param>
        /// <param name="ncState">The state to be used for non concurrent auth tasks.</param>
        public SetNicknameAuthTask
        (
            ILogger<SetNicknameAuthTask> logger,
            DuplicateResolver duplicates,
            NicknameResolver nickname,
            IScheduler scheduler,
            NonConcurrentTrigger.State ncState
        )
        {
            _duplicates = duplicates;
            _nickname = nickname;
            _scheduler = scheduler;
            _ncState = ncState;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<Result> ExecuteAsync(IAuthData data, CancellationToken ct = default)
        {
            var duplicate = await _duplicates.ResolveDuplicateAsync(data.LoadedUser, ct);
            if (duplicate.Type == DuplicityType.None)
            {
                return await EnqueueChange(data, ct);
            }

            return Result.FromSuccess();
        }

        private async Task<Result> EnqueueChange(IAuthData data, CancellationToken ct)
        {
            var nickname = await _nickname.ResolveNicknameAsync(data.LoadedUser, data.GuildUser, ct);

            if (nickname is null)
            {
                _logger.LogWarning("Could not obtain what the user's nickname should be");
                return Result.FromSuccess();
            }

            var jobData = new TypedJobData<CtuAuthNicknameSetJob>
                    (new JobKey("Auth", $"Set nickname <@{data.DbUser.DiscordId}>"))
                .AddData("Data", new CtuAuthNickname(data.DbUser.DiscordId, data.GuildId, nickname));
            var trigger = new NonConcurrentTrigger(new SimpleTrigger(), _ncState);

            var result = await _scheduler.ScheduleAsync(jobData, trigger, ct);

            return result.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(result);
        }
    }
}