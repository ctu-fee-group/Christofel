//
//   SetNicknameAuthTask.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.CtuAuth.JobQueue;
using Christofel.CtuAuth.Resolvers;
using Christofel.Helpers.JobQueue;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.CtuAuth.Auth.Tasks
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
        private readonly IJobQueue<CtuAuthNicknameSet> _jobQueue;
        private readonly ILogger _logger;
        private readonly NicknameResolver _nickname;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetNicknameAuthTask"/> class.
        /// </summary>
        /// <param name="jobQueue">The job queue.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="duplicates">The duplicate resolver.</param>
        /// <param name="nickname">The nickname resolver.</param>
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

        /// <inheritdoc />
        public async Task<Result> ExecuteAsync(IAuthData data, CancellationToken ct = default)
        {
            var duplicate = await _duplicates.ResolveDuplicateAsync(data.LoadedUser, ct);
            if (!duplicate.DuplicateFound)
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
                    data.GuildId,
                    nickname
                )
            );
            return Result.FromSuccess();
        }
    }
}