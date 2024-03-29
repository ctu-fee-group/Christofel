//
//   CtuAuthNicknameSetProcessor.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.BaseLib.Extensions;
using Christofel.Helpers.JobQueue;
using Christofel.Plugins.Lifetime;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;

namespace Christofel.CtuAuth.JobQueue
{
    /// <summary>
    /// Processor of nickname assigns, works on different thread.
    /// </summary>
    /// <remarks>
    /// Creates thread only if there is job assigned,
    /// if there isn't, the thread is freed (thread pool is used).
    /// </remarks>
    public class CtuAuthNicknameSetProcessor : ThreadPoolJobQueue<CtuAuthNicknameSet>
    {
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly ILifetime _lifetime;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CtuAuthNicknameSetProcessor"/> class.
        /// </summary>
        /// <param name="lifetime">The lifetime of the current plugin.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="guildApi">The guild api.</param>
        public CtuAuthNicknameSetProcessor
        (
            ICurrentPluginLifetime lifetime,
            ILogger<CtuAuthNicknameSetProcessor> logger,
            IDiscordRestGuildAPI guildApi
        )
            : base(lifetime, logger)
        {
            _logger = logger;
            _lifetime = lifetime;
            _guildApi = guildApi;
        }

        /// <inheritdoc />
        protected override async Task ProcessAssignJob(CtuAuthNicknameSet job)
        {
            var modifiedResult = await _guildApi.ModifyGuildMemberAsync
            (
                job.GuildId,
                job.UserId,
                job.Nickname,
                ct: _lifetime.Stopping
            );

            if (!modifiedResult.IsSuccess)
            {
                _logger.LogResultError
                (
                    modifiedResult,
                    $"Could not change nickname of <@{job.UserId}>, not going to retry."
                );
            }
        }
    }

    /// <summary>
    /// The job for <see cref="CtuAuthNicknameSetProcessor"/>.
    /// </summary>
    /// <param name="UserId"></param>
    /// <param name="GuildId"></param>
    /// <param name="Nickname"></param>
    public record CtuAuthNicknameSet(Snowflake UserId, Snowflake GuildId, string Nickname);
}