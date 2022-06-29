//
//   ReactCommandGroup.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Christofel.BaseLib.Extensions;
using Christofel.CommandsLib.Permissions;
using Microsoft.Extensions.Logging;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.Messages.Commands
{
    /// <summary>
    /// Handles /react command.
    /// </summary>
    [Ephemeral]
    public class ReactCommandGroup : CommandGroup
    {
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly ICommandContext _context;
        private readonly FeedbackService _feedbackService;
        private readonly ILogger<ReactCommandGroup> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReactCommandGroup"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The context of the current command.</param>
        /// <param name="feedbackService">The feedback service.</param>
        /// <param name="channelApi">The channel api.</param>
        public ReactCommandGroup
        (
            ILogger<ReactCommandGroup> logger,
            ICommandContext context,
            FeedbackService feedbackService,
            IDiscordRestChannelAPI channelApi
        )
        {
            _feedbackService = feedbackService;
            _context = context;
            _channelApi = channelApi;
            _logger = logger;
        }

        /// <summary>
        /// Handles /react command.
        /// </summary>
        /// <param name="messageId">The id of the message to react to.</param>
        /// <param name="emoji">The emoji to react with.</param>
        /// <param name="channel">The channel the message is in.</param>
        /// <returns>A result that may not have succeeded.</returns>
        [Command("react")]
        [RequirePermission("messages.react")]
        [Ephemeral]
        public async Task<Result> HandleReactAsync
        (
            [DiscordTypeHint(TypeHint.String)] [Description("Id of the message to react to")]
            Snowflake messageId,
            [Description("Emoji to react with")] string emoji,
            [Description("Channel where the message is located")] [DiscordTypeHint(TypeHint.Channel)]
            Snowflake? channel = null
        )
        {
            emoji = emoji.TrimStart('<').TrimEnd('>').TrimStart(':');
            var channelId = channel ?? _context.ChannelID;
            var result =
                await _channelApi.CreateReactionAsync(channelId, messageId, emoji, CancellationToken);

            Result<IReadOnlyList<IMessage>> feedbackResult;
            if (!result.IsSuccess)
            {
                _logger.LogResultError(result, $"Could not react with emoji {emoji}");

                feedbackResult =
                    await _feedbackService.SendContextualErrorAsync
                        ("There was an error and the reaction could not be added");
            }
            else
            {
                feedbackResult =
                    await _feedbackService.SendContextualSuccessAsync("Reaction added");
            }

            return feedbackResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(feedbackResult);
        }
    }
}