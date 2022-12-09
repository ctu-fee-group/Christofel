//
//   CtuAuthWarnMessageProcessor.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Christofel.BaseLib.Extensions;
using Christofel.Helpers.JobQueue;
using Christofel.Plugins.Lifetime;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;

namespace Christofel.Api.Ctu.JobQueue
{
    /// <summary>
    /// Processor for sending messages to the user.
    /// </summary>
    /// <remarks>
    /// Used for warning the user about inconsistent states.
    /// </remarks>
    public class CtuAuthWarnMessageProcessor : ThreadPoolJobQueue<CtuAuthWarnMessage>
    {
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IDiscordRestUserAPI _userApi;
        private readonly ILogger _logger;
        private readonly ILifetime _pluginLifetime;

        /// <summary>
        /// Initializes a new instance of the <see cref="CtuAuthWarnMessageProcessor"/> class.
        /// </summary>
        /// <param name="lifetime">The lifetime of the current plugin.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="userApi">The user api.</param>
        /// <param name="channelApi">The channel api.</param>
        public CtuAuthWarnMessageProcessor
        (
            ICurrentPluginLifetime lifetime,
            ILogger<CtuAuthWarnMessageProcessor> logger,
            IDiscordRestUserAPI userApi,
            IDiscordRestChannelAPI channelApi
        )
            : base(lifetime, logger)
        {
            _logger = logger;
            _pluginLifetime = lifetime;
            _userApi = userApi;
            _channelApi = channelApi;
        }

        /// <inheritdoc />
        protected override async Task ProcessAssignJob(CtuAuthWarnMessage job)
        {
            var dmResult = await _userApi.CreateDMAsync(job.UserId, _pluginLifetime.Stopping);
            if (!dmResult.IsSuccess)
            {
                _logger.LogResultError
                    (dmResult, $"Could not create DM channel for the user <@{job.UserId}>");
                return;
            }

            var messageResult = await _channelApi.CreateMessageAsync
                (dmResult.Entity.ID, job.Message, ct: _pluginLifetime.Stopping);
            if (!messageResult.IsSuccess)
            {
                _logger.LogResultError
                    (messageResult, $"Could not send DM to the user <@{job.UserId}>");
            }
        }
    }

    /// <summary>
    /// The job for <see cref="Christofel.Api.Ctu.JobQueue.CtuAuthNicknameSetProcessor"/>.
    /// </summary>
    /// <param name="UserId">The user to send the message to.</param>
    /// <param name="Message">The message to send.</param>
    public record CtuAuthWarnMessage(Snowflake UserId, string Message);
}