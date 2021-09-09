//
//   ReactCommandGroup.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Christofel.CommandsLib.Permissions;
using Microsoft.Extensions.Logging;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Core;
using Remora.Results;

namespace Christofel.Messages.Commands
{
    [Ephemeral]
    [DiscordDefaultPermission(false)]
    public class ReactCommandGroup : CommandGroup
    {
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly ICommandContext _context;
        private readonly FeedbackService _feedbackService;
        private readonly ILogger<ReactCommandGroup> _logger;

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

        // /react handler
        [Command("react")]
        [RequirePermission("messages.react")]
        public async Task<Result> HandleReactAsync
        (
            [DiscordTypeHint(TypeHint.String)] [Description("Id of the message to react to")]
            Snowflake messageId,
            [Description("Emoji to react with")] string emoji,
            [Description("Channel where the message is located")] [DiscordTypeHint(TypeHint.Channel)]
            Snowflake? channel = null
        )
        {
            var channelId = channel ?? _context.ChannelID;
            var result =
                await _channelApi.CreateReactionAsync(channelId, messageId, emoji, CancellationToken);

            Result<IReadOnlyList<IMessage>> feedbackResult;
            if (!result.IsSuccess)
            {
                _logger.LogError($"Could not react with emoji {emoji}: {result.Error.Message}");

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