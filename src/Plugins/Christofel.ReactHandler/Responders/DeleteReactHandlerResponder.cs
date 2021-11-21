//
//   DeleteReactHandlerResponder.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.ReactHandler.Database;
using Christofel.ReactHandler.Database.Models;
using Christofel.ReactHandler.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Gateway.Responders;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.ReactHandler.Responders
{
    /// <summary>
    /// Responder that deletes marked messages from the database if all the reactions are removed.
    /// </summary>
    public class DeleteReactHandlerResponder
        : IResponder<IMessageDelete>, IResponder<IMessageReactionRemove>,
            IResponder<IMessageReactionRemoveAll>, IResponder<IMessageReactionRemoveEmoji>
    {
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly ReactHandlerContext _dbContext;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteReactHandlerResponder"/> class.
        /// </summary>
        /// <param name="dbContext">The react handler database context.</param>
        /// <param name="channelApi">The channel api.</param>
        /// <param name="logger">The logger.</param>
        public DeleteReactHandlerResponder
        (
            ReactHandlerContext dbContext,
            IDiscordRestChannelAPI channelApi,
            ILogger<DeleteReactHandlerResponder> logger
        )
        {
            _logger = logger;
            _channelApi = channelApi;
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<Result> RespondAsync
        (
            IMessageDelete gatewayEvent,
            CancellationToken ct = default
        )
        {
            var matchingHandlers = _dbContext.HandleReacts
                .Where(x => x.ChannelId == gatewayEvent.ChannelID && x.MessageId == gatewayEvent.ID);

            return await DeleteHandlers(gatewayEvent.ChannelID, gatewayEvent.ID, matchingHandlers, ct);
        }

        /// <inheritdoc />
        public async Task<Result> RespondAsync
        (
            IMessageReactionRemove gatewayEvent,
            CancellationToken ct = default
        )
        {
            var shouldHandle = await _dbContext.HandleReacts.AnyAsync
            (
                x =>
                    x.ChannelId == gatewayEvent.ChannelID && x.MessageId == gatewayEvent.MessageID,
                ct
            );

            // We will handle only messages that are stored in database (database request is considered less costy than discord request).
            if (!shouldHandle)
            {
                return Result.FromSuccess();
            }

            var emoji = EmojiFormatter.GetEmojiString(gatewayEvent.Emoji);
            var messageResult =
                await _channelApi.GetChannelMessageAsync(gatewayEvent.ChannelID, gatewayEvent.MessageID, ct);

            if (!messageResult.IsSuccess)
            {
                return Result.FromError(messageResult);
            }

            var message = messageResult.Entity;
            if (!message.Reactions.IsDefined(out var reactions))
            {
                return Result.FromSuccess();
            }

            // We want only messages where this was the last remaining reaction
            var containsEmoji = reactions.Any(x => EmojiFormatter.GetEmojiString(x.Emoji) == emoji);
            if (containsEmoji)
            {
                return Result.FromSuccess();
            }

            var matchingHandlers = _dbContext.HandleReacts
                .Where
                (
                    x => x.ChannelId == gatewayEvent.ChannelID && x.MessageId == gatewayEvent.MessageID &&
                         x.Emoji == emoji
                );

            return await DeleteHandlers(gatewayEvent.ChannelID, gatewayEvent.MessageID, matchingHandlers, ct);
        }

        /// <inheritdoc />
        public Task<Result> RespondAsync
        (
            IMessageReactionRemoveAll gatewayEvent,
            CancellationToken ct = default
        )
        {
            var matchingHandlers = _dbContext.HandleReacts
                .Where(x => x.ChannelId == gatewayEvent.ChannelID && x.MessageId == gatewayEvent.MessageID);

            return DeleteHandlers(gatewayEvent.ChannelID, gatewayEvent.MessageID, matchingHandlers, ct);
        }

        /// <inheritdoc />
        public Task<Result> RespondAsync
        (
            IMessageReactionRemoveEmoji gatewayEvent,
            CancellationToken ct = default
        )
        {
            var emoji = EmojiFormatter.GetEmojiString(gatewayEvent.Emoji);
            var matchingHandlers = _dbContext.HandleReacts
                .Where
                (
                    x => x.ChannelId == gatewayEvent.ChannelID && x.MessageId == gatewayEvent.MessageID &&
                         x.Emoji == emoji
                );

            return DeleteHandlers(gatewayEvent.ChannelID, gatewayEvent.MessageID, matchingHandlers, ct);
        }

        private async Task<Result> DeleteHandlers
        (
            Snowflake channelId,
            Snowflake messageId,
            IQueryable<HandleReact> matchingHandlers,
            CancellationToken ct
        )
        {
            var toDelete = await matchingHandlers.ToListAsync(ct);
            _dbContext.RemoveRange(toDelete);
            await _dbContext.SaveChangesAsync(ct);

            if (toDelete.Count > 0)
            {
                _logger.LogInformation
                (
                    "Deleting {Count} react handlers for message {Message} in channel {Channel}",
                    toDelete.Count,
                    messageId,
                    channelId
                );
            }

            return Result.FromSuccess();
        }
    }
}