//
//   AutoPinResponder.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Common.Database.Models;
using Christofel.Common.Database.Models.Enums;
using Christofel.Common.Permissions;
using Christofel.Helpers.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Gateway.Responders;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.Enhancements.AutoPin;

/// <summary>
/// The responder to pin emoji.
/// </summary>
public class AutoPinResponder : IResponder<IMessageReactionAdd>, IResponder<IMessageUpdate>
{
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly IPermissionsResolver _permissionsResolver;
    private readonly ILogger<AutoPinResponder> _logger;
    private readonly AutoPinOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoPinResponder"/> class.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="channelApi">The channel api.</param>
    /// <param name="permissionsResolver">The permission resolver.</param>
    /// <param name="logger">The logger.</param>
    public AutoPinResponder
    (
        IOptionsSnapshot<AutoPinOptions> options,
        IDiscordRestChannelAPI channelApi,
        IPermissionsResolver permissionsResolver,
        ILogger<AutoPinResponder> logger
    )
    {
        _channelApi = channelApi;
        _permissionsResolver = permissionsResolver;
        _logger = logger;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<Result> RespondAsync(IMessageReactionAdd gatewayEvent, CancellationToken ct = default)
    {
        if (!IsPinEmoji(gatewayEvent.Emoji))
        {
            return Result.FromSuccess();
        }

        var userTarget = new DiscordTarget(gatewayEvent.UserID, TargetType.User);

        var messageResult = await _channelApi.GetChannelMessageAsync(gatewayEvent.ChannelID, gatewayEvent.MessageID, ct);
        if (!messageResult.IsDefined(out var message))
        {
            return Result.FromError(messageResult);
        }

        if (message.IsPinned)
        {
            return Result.FromSuccess();
        }

        bool pin = false;
        if (await _permissionsResolver.HasPermissionAsync("enhancements.autopin.override", userTarget, ct))
        {
            pin = true;
        }
        else
        {
            var neededEmojisResult = await GetNeededEmojisCountAsync(gatewayEvent.ChannelID, ct);
            if (!neededEmojisResult.IsDefined(out var neededEmojis))
            {
                return Result.FromError(neededEmojisResult);
            }

            if (neededEmojis == 0)
            {
                return Result.FromSuccess();
            }

            var emojisCountResult = GetEmojisCount(message);
            if (!emojisCountResult.IsDefined(out var emojisCount))
            {
                return Result.FromError(emojisCountResult);
            }

            pin = emojisCount >= neededEmojis;
        }

        if (pin)
        {
            _logger.LogInformation
            (
                "Autopinned a message {Message} in channel {Channel}",
                gatewayEvent.MessageID,
                $"<#{gatewayEvent.ChannelID}>"
            );

            var emojiString = EmojiFormatter.GetEmojiString(gatewayEvent.Emoji);

            // Not checking the result, does not matter so much.
            await _channelApi.CreateReactionAsync(gatewayEvent.ChannelID, gatewayEvent.MessageID, emojiString, ct);

            return await _channelApi.PinMessageAsync(gatewayEvent.ChannelID, gatewayEvent.MessageID, "Auto pin", ct);
        }

        return Result.FromSuccess();
    }

    /// <inheritdoc />
    public async Task<Result> RespondAsync(IMessageUpdate gatewayEvent, CancellationToken ct = default)
    {
        if (!gatewayEvent.IsPinned.IsDefined(out var pinned) || pinned)
        {
            return Result.FromSuccess();
        }

        if (!gatewayEvent.ChannelID.IsDefined(out var channelId))
        {
            return Result.FromSuccess();
        }

        if (!gatewayEvent.ID.IsDefined(out var messageId))
        {
            return Result.FromSuccess();
        }

        foreach (var emoji in _options.AutoPinEmojis)
        {
            await _channelApi.DeleteOwnReactionAsync(channelId, messageId, emoji, ct);
        }

        return Result.FromSuccess();
    }

    private bool IsPinEmoji(IPartialEmoji emoji)
        => _options.AutoPinEmojis.Contains(EmojiFormatter.GetEmojiString(emoji));

    private Result<int> GetEmojisCount(IMessage message)
    {
        if (!message.Reactions.IsDefined(out var reactions))
        {
            return Result<int>.FromSuccess(0);
        }

        return reactions.Aggregate
        (
            0,
            (acc, x) =>
                acc + (IsPinEmoji(x.Emoji)
                    ? x.Count
                    : 0)
        );
    }

    private async Task<Result<int>> GetNeededEmojisCountAsync(Snowflake channelId, CancellationToken ct)
    {
        var channelResult = await _channelApi.GetChannelAsync(channelId, ct);
        if (!channelResult.IsDefined(out var channel))
        {
            return Result<int>.FromError(channelResult);
        }

        int neededEmojis = _options.MinimumCount;

        if (_options.MinimumCountOverrides is null)
        {
            return neededEmojis;
        }

        if (channel.ParentID.IsDefined(out var parentId) &&
            _options.MinimumCountOverrides.ContainsKey(parentId.Value))
        {
            neededEmojis = _options.MinimumCountOverrides[parentId.Value];
        }

        if (_options.MinimumCountOverrides.ContainsKey(channelId))
        {
            neededEmojis = _options.MinimumCountOverrides[channelId];
        }

        return neededEmojis;
    }
}