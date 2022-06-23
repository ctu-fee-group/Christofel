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
using Remora.Rest.Core;
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
        /// <returns>A result of the command that may not have succeeded.</returns>
        [Command("send")]
        [Description("Send a message")]
        [RequirePermission("messages.echo.send")]
        public async Task<Result> HandleEcho
        (
            [Description("Text of the message to send"), Greedy]
            string text
        )
        {
            var channelId = _context.ChannelID;
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
        /// <returns>A result of the command that may not have succeeded.</returns>
        [Command("edit")]
        [Description("Edit a message sent by the bot")]
        [RequirePermission("messages.echo.edit")]
        public async Task<Result> HandleEdit
        (
            [Description("What message to edit")] [DiscordTypeHint(TypeHint.String)]
            Snowflake messageId,
            [Description("New text of the message"), Greedy]
            string text
        )
        {
            var channelId = _context.ChannelID;
            var messageResult =
                await _channelApi.GetChannelMessageAsync(channelId, messageId, CancellationToken);
            if (!messageResult.IsSuccess)
            {
                // Ignore as message not loaded is more critical
                await _feedbackService.SendContextualErrorAsync("Could not load the message", ct: CancellationToken);
                return Result.FromError(messageResult);
            }

            var editResult = await _channelApi.EditMessageAsync
            (
                channelId,
                messageId,
                text,
                allowedMentions: AllowedMentionsHelper.None,
                ct: CancellationToken
            );
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

        /// <summary>
        /// Handles /echo source.
        /// </summary>
        /// <remarks>
        /// Gets the source of the message.
        /// </remarks>
        /// <param name="message">The id of the message to get source of.</param>
        /// <param name="channel">The id of the channel the message is in.</param>
        /// <returns>A result that may have failed.</returns>
        [Command("source")]
        [Description("Get the source of the given message.")]
        [RequirePermission("messages.echo.source")]
        public async Task<Result> HandleSource
        (
            [Description("The id of the message to get source to.")]
            [DiscordTypeHint(TypeHint.String)]
            Snowflake message,
            [Description("The channel the message is in.")]
            [DiscordTypeHint(TypeHint.Channel)]
            Snowflake? channel = default
        )
        {
            var channelId = channel ?? _context.ChannelID;
            var messageResult = await _channelApi.GetChannelMessageAsync(channelId, message, CancellationToken);
            if (!messageResult.IsDefined(out var fetchedMessage))
            {
                return Result.FromError(messageResult);
            }

            var createdMessageResult = await _channelApi.CreateMessageAsync
            (
                _context.ChannelID,
                "```\n" + fetchedMessage.Content.Replace("```", "\\`\\`\\`") + "\n```",
                ct: CancellationToken
            );

            await _feedbackService.SendContextualSuccessAsync("Source is below.");

            return createdMessageResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(createdMessageResult);
        }
    }
}