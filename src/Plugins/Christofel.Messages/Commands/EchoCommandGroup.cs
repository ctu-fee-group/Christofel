//
//   EchoCommandGroup.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Threading.Tasks;
using Christofel.CommandsLib.Permissions;
using Christofel.Helpers.Helpers;
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
    /// <summary>
    /// Command group handling /echo command for creating and editing echo messages.
    /// </summary>
    [Group("echo")]
    [Description]
    [Ephemeral]
    [RequirePermission("messages.echo")]
    public class EchoCommandGroup : CommandGroup
    {
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly ICommandContext _context;
        private readonly FeedbackService _feedbackService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EchoCommandGroup"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="channelApi">The channel api.</param>
        /// <param name="context">The context of the current command.</param>
        /// <param name="feedbackService">The feedback service.</param>
        public EchoCommandGroup
        (
            ILogger<EchoCommandGroup> logger,
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

        /// <summary>
        /// Handles /echo send.
        /// </summary>
        /// <remarks>
        /// Sends given message to the given channel.
        /// </remarks>
        /// <param name="text">The message to be sent.</param>
        /// <param name="channel">The channel to send the message into. If omitted, the channel from the context will be used.</param>
        /// <returns>A result of the command that may not have succeeded.</returns>
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

        /// <summary>
        /// Handles /echo edit command.
        /// </summary>
        /// <remarks>
        /// Edits the given message.
        /// </remarks>
        /// <param name="messageId">The id of the message to edit.</param>
        /// <param name="text">The text to edit the content with.</param>
        /// <param name="channel">The channel where the message is located.</param>
        /// <returns>A result of the command that may not have succeeded.</returns>
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