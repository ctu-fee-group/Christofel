//
//   SetNicknameAuthTask.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu.JobQueue;
using Christofel.Api.Ctu.Resolvers;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Tasks
{
    public class SetNicknameAuthTask : IAuthTask
    {
        private readonly DuplicateResolver _duplicates;
        private readonly IJobQueue<CtuAuthNicknameSet> _jobQueue;
        private readonly ILogger _logger;
        private readonly NicknameResolver _nickname;

        public SetNicknameAuthTask
        (
            IJobQueue<CtuAuthNicknameSet> jobQueue,
            ILogger<SetNicknameAuthTask> logger,
            DuplicateResolver duplicates,
            NicknameResolver nickname
        )
        {
            _duplicates = duplicates;
            _nickname = nickname;

            _jobQueue = jobQueue;
            _logger = logger;
        }

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

            _jobQueue.EnqueueJob
            (
                new CtuAuthNicknameSet
                (
                    data.DbUser.DiscordId,
                    data.GuildId, nickname
                )
            );
            return Result.FromSuccess();
        }
    }
}