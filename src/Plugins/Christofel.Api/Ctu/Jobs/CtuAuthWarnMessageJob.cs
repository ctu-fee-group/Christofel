//
//   CtuAuthWarnMessageJob.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Christofel.Scheduling;
using Christofel.Scheduling.Extensions;
using Christofel.Scheduling.Recoverable;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Results;

namespace Christofel.Api.Ctu.Jobs
{
    /// <summary>
    /// Job for sending messages to the user.
    /// </summary>
    /// <remarks>
    /// Used for warning the user about inconsistent states.
    /// </remarks>
    public class CtuAuthWarnMessageJob : IDataJob<CtuAuthWarnMessage>
    {
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IDiscordRestUserAPI _userApi;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CtuAuthWarnMessageJob"/> class.
        /// </summary>
        /// <param name="lifetime">The lifetime of the current plugin.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="userApi">The user api.</param>
        /// <param name="channelApi">The channel api.</param>
        /// <param name="warnMessage">The warn message.</param>
        public CtuAuthWarnMessageJob
        (
            ILogger<CtuAuthWarnMessageJob> logger,
            IDiscordRestUserAPI userApi,
            IDiscordRestChannelAPI channelApi,
            CtuAuthWarnMessage warnMessage
        )
        {
            Data = warnMessage;
            _logger = logger;
            _userApi = userApi;
            _channelApi = channelApi;
        }

        /// <inheritdoc />
        public async Task<Result> ExecuteAsync(IJobContext jobContext, CancellationToken ct = default)
        {
            var dmResult = await _userApi.CreateDMAsync(Data.UserId, ct);
            if (!dmResult.IsSuccess)
            {
                _logger.LogResult(dmResult, $"Could not create DM channel for <@{Data.UserId}> to warn him about his roles.");
                return Result.FromSuccess();
            }

            var messageResult = await _channelApi.CreateMessageAsync(dmResult.Entity.ID, Data.Message, ct: ct);
            if (!messageResult.IsSuccess)
            {
                _logger.LogResult(messageResult, $"Could not send message in DM channel for <@{Data.UserId}> to warn him about his roles.");
                return Result.FromSuccess();
            }

            return Result.FromSuccess();
        }

        /// <inheritdoc />
        public CtuAuthWarnMessage Data { get; set; }
    }
}