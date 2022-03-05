//
//   CtuAuthNicknameSetJob.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Christofel.Scheduling;
using Christofel.Scheduling.Recoverable;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Results;

namespace Christofel.Api.Ctu.Jobs
{
    /// <summary>
    /// Job that sets nickname of the specified user.
    /// </summary>
    public class CtuAuthNicknameSetJob : IDataJob<CtuAuthNickname>
    {
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CtuAuthNicknameSetJob"/> class.
        /// </summary>
        /// <param name="data">The job data.</param>
        /// <param name="guildApi">The guild api.</param>
        /// <param name="logger">The logger.</param>
        public CtuAuthNicknameSetJob
            (CtuAuthNickname data, IDiscordRestGuildAPI guildApi, ILogger<CtuAuthNicknameSetJob> logger)
        {
            _logger = logger;
            _guildApi = guildApi;
            Data = data;
        }

        /// <inheritdoc />
        public async Task<Result> ExecuteAsync(IJobContext jobContext, CancellationToken ct = default)
        {
            var data = Data;

            var modifiedResult = await _guildApi.ModifyGuildMemberAsync
            (
                data.GuildId,
                data.UserId,
                data.Nickname,
                ct: ct
            );

            if (!modifiedResult.IsSuccess)
            {
                _logger.LogWarning
                (
                    "Could not change nickname of <@{UserId}>, not going to retry. {Error}",
                    data.UserId,
                    modifiedResult.Error.Message
                );
            }

            return Result.FromSuccess();
        }

        /// <inheritdoc />
        public CtuAuthNickname Data { get; set; }
    }
}