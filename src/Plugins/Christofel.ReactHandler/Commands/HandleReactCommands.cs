//
//   HandleReactCommands.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Christofel.CommandsLib.Permissions;
using Christofel.ReactHandler.Database;
using Christofel.ReactHandler.Database.Models;
using Christofel.ReactHandler.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.ReactHandler.Commands
{
    /// <summary>
    /// Handles /handlereact unmark|mark|show commands.
    /// </summary>
    [Group("handlereact")]
    [RequirePermission("reacthandler.handlereact")]
    [Description("Mark messages so that the bot handles reacts on them")]
    [DiscordDefaultPermission(false)]
    [Ephemeral]
    public class HandleReactCommands : CommandGroup
    {
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly ICommandContext _commandContext;
        private readonly ReactHandlerContext _dbContext;
        private readonly FeedbackService _feedbackService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandleReactCommands"/> class.
        /// </summary>
        /// <param name="dbContext">The react handler database context.</param>
        /// <param name="commandContext">The context of the current command.</param>
        /// <param name="feedbackService">The feedback service.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="channelApi">The channel api.</param>
        public HandleReactCommands
        (
            ReactHandlerContext dbContext,
            ICommandContext commandContext,
            FeedbackService feedbackService,
            ILogger<HandleReactCommands> logger,
            IDiscordRestChannelAPI channelApi
        )
        {
            _logger = logger;
            _commandContext = commandContext;
            _dbContext = dbContext;
            _feedbackService = feedbackService;
            _channelApi = channelApi;
        }

        /// <summary>
        /// Handles /handlereact unmark command.
        /// </summary>
        /// <param name="messageId">The message to react to.</param>
        /// <param name="reactEmoji">The emoji to react to.</param>
        /// <param name="channel">The channel the message is located in.</param>
        /// <returns>A result that may not have succeeded.</returns>
        [Command("unmark")]
        [Description("Unmark specified message to not be handled by the bot on user reaction.")]
        public async Task<Result> HandleReactRemove
        (
            [Description("Message that the bot should not react to")]
            Snowflake messageId,
            [Description("Emoji that the bot should not react to anymore")]
            string reactEmoji,
            [Description("Channel that the message is in. If omitted, current channel will be used")]
            [DiscordTypeHint(TypeHint.Channel)]
            IPartialChannel? channel = default
        )
        {
            var channelId = channel?.ID ?? _commandContext.ChannelID;
            if (!channelId.HasValue)
            {
                return new InvalidOperationError("Channel id must not be empty");
            }

            List<HandleReact> matchingHandlers;
            try
            {
                matchingHandlers = await _dbContext.HandleReacts
                    .Where(x => x.ChannelId == channelId && x.MessageId == messageId && x.Emoji == reactEmoji)
                    .ToListAsync(CancellationToken);

                _dbContext.RemoveRange(matchingHandlers);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not match/save reaction handlers that should be deleted");
                await _feedbackService.SendContextualErrorAsync
                (
                    "Could not delete matching handlers from database.",
                    ct: CancellationToken
                );
                return new ExceptionError(e);
            }

            var feedbackResult =
                await _feedbackService.SendContextualSuccessAsync
                (
                    $"Deleted {matchingHandlers.Count} handlers.",
                    ct: CancellationToken
                );

            return feedbackResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(feedbackResult);
        }

        /// <summary>
        /// Handles /handlereact mark command.
        /// </summary>
        /// <param name="messageId">The message to react to.</param>
        /// <param name="reactEmoji">The emoji to react to.</param>
        /// <param name="entity">The entity that should be assigned. Has to be Role or Channel.</param>
        /// <param name="channel">The channel the message is in.</param>
        /// <returns>A result that may not have succeeded.</returns>
        [Command("mark")]
        [Description("Mark specified message to be handled by the bot to add channel or role on user reaction.")]
        public async Task<Result> HandleReactAdd
        (
            [Description("Message that the bot should react to")]
            Snowflake messageId,
            [Description("Emoji that the bot should react to")]
            string reactEmoji,
            [Description("Entity to be added/removed on reaction")]
            OneOf<IRole, IPartialChannel> entity,
            [Description("Channel that the message is in. If omitted, current channel will be used")]
            [DiscordTypeHint(TypeHint.Channel)]
            IPartialChannel? channel = default
        )
        {
            var channelId = channel?.ID ?? _commandContext.ChannelID;
            if (!channelId.HasValue)
            {
                return new InvalidOperationError("Channel id must not be empty");
            }

            // 1. react to the message
            var reactionResult =
                await _channelApi.CreateReactionAsync
                (
                    channelId.Value,
                    messageId,
                    reactEmoji.TrimStart('<').TrimEnd('>').TrimStart(':'),
                    CancellationToken
                );
            if (!reactionResult.IsSuccess)
            {
                await _feedbackService.SendContextualErrorAsync
                (
                    $"Could not react to the message, check permissions and whether you sent correct emoji. {reactionResult.Error.Message}",
                    ct: CancellationToken
                );
                return reactionResult;
            }

            // 2. save to database
            try
            {
                var handleReact = new HandleReact
                {
                    ChannelId = channelId.Value,
                    Emoji = reactEmoji,
                    EntityId = entity.IsT0
                        ? entity.AsT0.ID
                        : entity.AsT1.ID.Value,
                    MessageId = messageId,
                    Type = entity.IsT0
                        ? HandleReactType.Role
                        : HandleReactType.Channel,
                };

                _dbContext.Add(handleReact);
                await _dbContext.SaveChangesAsync(CancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not save reaction handler to database");
                await _feedbackService.SendContextualErrorAsync("Could not save data to the database.");
                return e;
            }

            // 3. send information to the user
            var feedbackResult =
                await _feedbackService.SendContextualSuccessAsync
                    ("Correctly marked the message and reacted with the specified emoji");

            return feedbackResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(feedbackResult);
        }

        /// <summary>
        /// Handles /handlereact show command.
        /// </summary>
        /// <param name="messageId">The message to show information about.</param>
        /// <param name="reactEmoji">The emoji to show information about.</param>
        /// <param name="channel">The channel where the message is in.</param>
        /// <returns>A result that may not have succeeded.</returns>
        [Command("show")]
        [Description("Show information about reacts of the specified message that are handled by the bot.")]
        public async Task<Result> HandleReactShow
        (
            [Description("Message target")]
            Snowflake messageId,
            [Description("Filter for emoji, if empty, all will be shown")]
            string? reactEmoji = null,
            [Description("Channel that the message is in. If omitted, current channel will be used")]
            [DiscordTypeHint(TypeHint.Channel)]
            IPartialChannel? channel = default
        )
        {
            var channelId = channel?.ID ?? _commandContext.ChannelID;
            if (!channelId.HasValue)
            {
                return new InvalidOperationError("Channel id must not be empty");
            }

            List<HandleReact> matchingHandlers;
            try
            {
                var matchingHandlersQuery = _dbContext.HandleReacts
                    .Where(x => x.ChannelId == channelId.Value && x.MessageId == messageId);

                if (reactEmoji is not null)
                {
                    matchingHandlersQuery = matchingHandlersQuery
                        .Where(x => x.Emoji == reactEmoji);
                }

                matchingHandlers = await matchingHandlersQuery.ToListAsync(CancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogInformation(e, "Could not retrieve reaction handlers");
                await _feedbackService.SendContextualErrorAsync
                (
                    "Could not retrieve reaction handlers from database.",
                    ct: CancellationToken
                );
                return new ExceptionError(e);
            }

            var groupedMessages = matchingHandlers.GroupBy(x => x.Emoji, HandleReactFormatter.FormatHandlerTarget)
                .Select(x => $"{x.Key}: {string.Join(", ", x)}");
            var message = string.Join("\n", groupedMessages);

            var feedbackResult =
                await _feedbackService.SendContextualInfoAsync($"Reaction handlers attached:\n {message}");

            return feedbackResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(feedbackResult);
        }
    }
}