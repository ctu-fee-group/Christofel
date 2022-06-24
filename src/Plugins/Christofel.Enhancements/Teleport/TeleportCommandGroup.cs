//
//  TeleportCommandGroup.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Christofel.CommandsLib.Permissions;
using Christofel.Common.Database.Models;
using Christofel.Common.Database.Models.Enums;
using Christofel.Common.Permissions;
using Christofel.Helpers.Helpers;
using Christofel.Helpers.Permissions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneOf.Types;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Messages;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Commands.Results;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.Enhancements.Teleport;

/// <summary>
/// The teleport command class.
/// </summary>
public class TeleportCommandGroup : CommandGroup
{
    private readonly TeleportOptions _options;
    private readonly IDiscordRestGuildAPI _guildApi;
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly ICommandContext _commandContext;
    private readonly MemberPermissionResolver _permissionsResolver;
    private readonly FeedbackService _feedbackService;
    private readonly ILogger<TeleportCommandGroup> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TeleportCommandGroup"/> class.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="guildApi">The guild api.</param>
    /// <param name="channelApi">The channel api.</param>
    /// <param name="commandContext">The command context.</param>
    /// <param name="permissionsResolver">The permissions resolver.</param>
    /// <param name="feedbackService">The feedback service.</param>
    /// <param name="logger">The logger.</param>
    public TeleportCommandGroup
    (
        IOptionsSnapshot<TeleportOptions> options,
        IDiscordRestGuildAPI guildApi,
        IDiscordRestChannelAPI channelApi,
        ICommandContext commandContext,
        MemberPermissionResolver permissionsResolver,
        FeedbackService feedbackService,
        ILogger<TeleportCommandGroup> logger
    )
    {
        _options = options.Value;
        _guildApi = guildApi;
        _channelApi = channelApi;
        _commandContext = commandContext;
        _permissionsResolver = permissionsResolver;
        _feedbackService = feedbackService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the teleport command.
    /// </summary>
    /// <param name="channel">The id of the channel to teleport to.</param>
    /// <returns>A result that may not have succeeded.</returns>
    [Command("teleport")]
    [RequirePermission("enhancements.teleport")]
    [Ephemeral]
    public async Task<Result> HandleTeleportAsync
    (
        [Description("The id of the channel to teleport to.")] [DiscordTypeHint(TypeHint.Channel)]
        Snowflake channel
    )
    {
        var permissionsResult = await CheckPermissionsAsync(channel, CancellationToken);
        if (!permissionsResult.IsSuccess || _commandContext.ChannelID == channel)
        {
            await _feedbackService.SendContextualErrorAsync("Sending teleport messages failed.", ct: CancellationToken);
            return permissionsResult;
        }

        if (!_commandContext.GuildID.IsDefined(out var guildId))
        {
            return Result.FromSuccess();
        }

        // 0. check permissions
        // 1. send message to target channel.
        // 2. send message to current channel linking the target message.
        // 3. link the current channel in the message sent in (1)
        // 4. send confirmation to the user.
        // Done.
        Snowflake userId = _commandContext.User.ID;
        Snowflake currentChannelId = _commandContext.ChannelID;
        var messageInTargetResult = await SendMessageAsync
        (
            guildId,
            userId,
            channel,
            currentChannelId,
            null,
            _options.MessageFrom,
            CancellationToken
        );

        if (!messageInTargetResult.IsDefined(out var messageInTarget))
        {
            await _feedbackService.SendContextualErrorAsync("Sending teleport messages failed.", ct: CancellationToken);
            return Result.FromError(messageInTargetResult);
        }

        var messageInCurrentResult = await SendMessageAsync
        (
            guildId,
            userId,
            currentChannelId,
            channel,
            messageInTarget,
            _options.MessageTo,
            CancellationToken
        );

        if (!messageInCurrentResult.IsDefined(out var messageInCurrent))
        {
            await _feedbackService.SendContextualErrorAsync("Sending teleport messages failed.", ct: CancellationToken);
            return Result.FromError(messageInCurrentResult);
        }

        messageInTargetResult = await EditMessageAsync
        (
            guildId,
            userId,
            channel,
            messageInTarget,
            currentChannelId,
            messageInCurrent,
            _options.MessageFrom,
            CancellationToken
        );

        if (!messageInTargetResult.IsDefined(out messageInTarget))
        {
            await _feedbackService.SendContextualErrorAsync("Sending teleport messages failed.", ct: CancellationToken);
            return Result.FromError(messageInTargetResult);
        }

        var feedbackResult = await _feedbackService.SendContextualSuccessAsync
            ("Teleport messages sent.", ct: CancellationToken);

        _logger.LogInformation
        (
            "Created a teleport from <#{ChannelFrom}> to <#{ChannelTo}>. See:\n{MessageFrom}\n{MessageTo}",
            _commandContext.ChannelID,
            channel,
            GetMessageUrl(guildId, _commandContext.ChannelID, messageInCurrent),
            GetMessageUrl(guildId, channel, messageInTarget)
        );

        return feedbackResult.IsSuccess ? Result.FromSuccess() : Result.FromError(feedbackResult);
    }

    private async Task<global::Remora.Results.Result<Snowflake>> SendMessageAsync
    (
        Snowflake guildId,
        Snowflake userId,
        Snowflake channelId,
        Snowflake referenceChannelId,
        Snowflake? referenceMessageId,
        string message,
        CancellationToken ct
    )
    {
        var creationResult = await _channelApi.CreateMessageAsync
        (
            channelId,
            GetMessageContent(guildId,  userId, referenceChannelId, referenceMessageId, message),
            allowedMentions: AllowedMentionsHelper.None,
            ct: ct
        );

        return creationResult.IsSuccess
            ? creationResult.Entity.ID
            : global::Remora.Results.Result<Snowflake>.FromError(creationResult);
    }

    private async Task<global::Remora.Results.Result<Snowflake>> EditMessageAsync
    (
        Snowflake guildId,
        Snowflake userId,
        Snowflake channelId,
        Snowflake messageId,
        Snowflake referenceChannelId,
        Snowflake referenceMessageId,
        string message,
        CancellationToken ct
    )
    {
        var creationResult = await _channelApi.EditMessageAsync
        (
            channelId,
            messageId,
            GetMessageContent
            (
                guildId,
                userId,
                referenceChannelId,
                referenceMessageId,
                message
            ),
            ct: ct
        );
        return creationResult.IsSuccess
            ? creationResult.Entity.ID
            : global::Remora.Results.Result<Snowflake>.FromError(creationResult);
    }

    private string GetMessageContent
    (
        Snowflake guildId,
        Snowflake userId,
        Snowflake referenceChannelId,
        Snowflake? referenceMessageId,
        string message
    )
    {
        var messageHref = referenceMessageId is null
            ? "LINKING"
            : GetMessageUrl(guildId, referenceChannelId, referenceMessageId.Value);

        return message
            .Replace("{Channel}", $"<#{referenceChannelId}>")
            .Replace("{User}", $"<@{userId}>")
            .Replace("{Reference}", messageHref);
    }

    private string GetMessageUrl(Snowflake guildId, Snowflake referenceChannelId, Snowflake referenceMessageId)
        => $"https://discord.com/channels/{guildId}/{referenceChannelId}/{referenceMessageId}";

    private async Task<Result> CheckPermissionsAsync(Snowflake channelId, CancellationToken ct)
    {
        var overridePermissionResult = await _permissionsResolver.HasPermissionAsync
            ("enhancements.teleport.override", _commandContext.User.ID, _commandContext.GuildID, ct: ct);
        if (!overridePermissionResult.IsDefined(out var overridePermission))
        {
            return Result.FromError(overridePermissionResult);
        }

        if (overridePermission)
        {
            return Result.FromSuccess();
        }

        if (!_commandContext.GuildID.IsDefined(out var guildId))
        {
            return new InvalidOperationException("Teleport used outside of guild.");
        }

        var rolesResult = await _guildApi.GetGuildRolesAsync(guildId, ct);
        if (!rolesResult.IsDefined(out var roles))
        {
            return Result.FromError(rolesResult);
        }

        var channelResult = await _channelApi.GetChannelAsync(channelId, ct);
        if (!channelResult.IsDefined(out var channel))
        {
            return Result.FromError(channelResult);
        }

        var memberResult = await _guildApi.GetGuildMemberAsync(guildId, _commandContext.User.ID, ct);
        if (!memberResult.IsDefined(out var member))
        {
            return Result.FromError(memberResult);
        }

        var everyoneRole = roles.First(x => x.ID == guildId);
        var memberRoles = roles.Where(x => member.Roles.Contains(x.ID)).ToArray();
        var overwrites = channel.PermissionOverwrites.HasValue ? channel.PermissionOverwrites.Value : null;

        var permissions = DiscordPermissionSet.ComputePermissions
            (_commandContext.User.ID, everyoneRole, memberRoles, overwrites);
        return permissions.HasPermission(DiscordPermission.SendMessages)
            ? Result.FromSuccess()
            : new InvalidOperationError
                ($"Insufficient permissions to use teleport to the specified channel ({guildId}).");
    }
}