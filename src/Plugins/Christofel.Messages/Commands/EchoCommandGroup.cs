//
//   EchoCommandGroup.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Threading.Tasks;
using Christofel.BaseLib.Implementations.Helpers;
using Christofel.CommandsLib.Permissions;
using Microsoft.Extensions.Logging;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Core;
using Remora.Results;

namespace Christofel.Messages.Commands
{
    [Group("echo")]
    [Description]
    [Ephemeral]
    [RequirePermission("messages.echo")]
    public class EchoCommandGroup : CommandGroup
    {
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly ICommandContext _context;
        private readonly FeedbackService _feedbackService;
        private readonly ILogger<ReactCommandGroup> _logger;

        public EchoCommandGroup
        (
            ILogger<ReactCommandGroup> logger,
            IDiscordRestChannelAPI channelApi,
            ICommandContext context,
            FeedbackService feedbackService
        )
        {
            _context = context;
            _feedbackService = feedbackService;
            _channelApi = channelApi;
            _logger = logger;
        }

        [Command("send")]
        [Description("Send a message")]
        [RequirePermission("messages.echo.send")]
        public async Task<Result> HandleEcho
        (
            [Description("Text of the message to send")]
            string text,
            [Description("Where to send the message. Default is current channel")] [DiscordTypeHint(TypeHint.Channel)]
            Snowflake? channel = null
        )
        {
            var channelId = channel ?? _context.ChannelID;
            var messageResult = await _channelApi.CreateMessageAsync
                (channelId, text, allowedMentions: AllowedMentionsHelper.None, ct: CancellationToken);
            if (!messageResult.IsSuccess)
            {
                // Ignore as message not sent is more critical
                await _feedbackService.SendContextualErrorAsync
                (
                    "Could not send the message, check permissions",
                    ct: CancellationToken
                );
                return Result.FromError(messageResult);
            }

            var feedbackResult =
                await _feedbackService.SendContextualSuccessAsync("Message sent", ct: CancellationToken);
            return feedbackResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(feedbackResult);
        }

        [Command("edit")]
        [Description("Edit a message sent by the bot")]
        [RequirePermission("messages.echo.edit")]
        public async Task<Result> HandleEdit
        (
            [Description("What message to edit")] [DiscordTypeHint(TypeHint.String)]
            Snowflake messageId,
            [Description("New text of the message")]
            string text,
            [Description("Where to send the message. Default is current channel")] [DiscordTypeHint(TypeHint.Channel)]
            Snowflake? channel = null
        )
        {
            var channelId = channel ?? _context.ChannelID;
            var messageResult =
                await _channelApi.GetChannelMessageAsync(channelId, messageId, CancellationToken);
            if (!messageResult.IsSuccess)
            {
                // Ignore as message not loaded is more critical
                await _feedbackService.SendContextualErrorAsync("Could not load the message", ct: CancellationToken);
                return Result.FromError(messageResult);
            }

            var editResult = await _channelApi.EditMessageAsync
                (channelId, messageId, text, allowedMentions: AllowedMentionsHelper.None, ct: CancellationToken);
            if (!editResult.IsSuccess)
            {
                // Ignore as message not modified is more critical
                await _feedbackService.SendContextualErrorAsync("Could not edit the message", ct: CancellationToken);
                return Result.FromError(messageResult);
            }

            var feedbackResult = await _feedbackService.SendContextualSuccessAsync
                ("Message edited", ct: CancellationToken);

            return feedbackResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(feedbackResult);
        }
    }
}