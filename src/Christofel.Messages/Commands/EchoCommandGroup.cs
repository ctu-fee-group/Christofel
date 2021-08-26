using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Christofel.CommandsLib;
using Microsoft.Extensions.Logging;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Conditions;
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
        private readonly ILogger<ReactCommandGroup> _logger;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly FeedbackService _feedbackService;
        private readonly ICommandContext _context;

        public EchoCommandGroup(ILogger<ReactCommandGroup> logger, IDiscordRestChannelAPI channelApi,
            ICommandContext context, FeedbackService feedbackService)
        {
            _context = context;
            _feedbackService = feedbackService;
            _channelApi = channelApi;
            _logger = logger;
        }

        [Command("send")]
        [Description("Send a message")]
        [RequirePermission("messages.echo.send")]
        public async Task<Result> HandleEcho(
            [Description("Text of the message to send")]
            string text,
            [Description("Where to send the message. Default is current channel"), DiscordTypeHint(TypeHint.Channel)]
            Optional<Snowflake> channel = default)
        {
            var channelId = channel.HasValue ? channel.Value : _context.ChannelID;
            var messageResult = await _channelApi.CreateMessageAsync(channel.Value, text, ct: CancellationToken);
            if (!messageResult.IsSuccess)
            {
                // Ignore as message not sent is more critical
                await _feedbackService.SendContextualErrorAsync("Could not send the message, check permissions",
                    ct: CancellationToken);
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
        public async Task<Result> HandleEdit(
            [Description("What message to edit"), DiscordTypeHint(TypeHint.String)]
            Snowflake messageId,
            [Description("New text of the message")]
            string text,
            [Description("Where to send the message. Default is current channel"), DiscordTypeHint(TypeHint.Channel)]
            Optional<Snowflake> channel = default)
        {
            var channelId = channel.HasValue ? channel.Value : _context.ChannelID;

            var messageResult =
                await _channelApi.GetChannelMessageAsync(channelId, messageId, CancellationToken);
            if (!messageResult.IsSuccess)
            {
                // Ignore as message not loaded is more critical
                await _feedbackService.SendContextualErrorAsync("Could not load the message");
                return Result.FromError(messageResult);
            }

            var editResult = await _channelApi.EditMessageAsync(channelId, messageId, text, ct: CancellationToken);
            if (!editResult.IsSuccess)
            {
                // Ignore as message not modified is more critical
                await _feedbackService.SendContextualErrorAsync("Could not edit the message");
                return Result.FromError(messageResult);
            }

            return Result.FromSuccess();
        }
    }
}