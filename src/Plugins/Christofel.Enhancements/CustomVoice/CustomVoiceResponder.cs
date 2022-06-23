//
//   CustomVoiceResponder.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Common.Database.Models;
using Christofel.Common.Database.Models.Enums;
using Christofel.Common.Permissions;
using Christofel.Enhancements.Extensions;
using Christofel.Helpers.Storages;
using Christofel.Plugins.Lifetime;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Gateway.Responders;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.Enhancements.CustomVoice;

/// <summary>
/// Creates voice or stage channel when someone connects to the correct channel.
/// </summary>
public class CustomVoiceResponder : IResponder<IVoiceStateUpdate>
{
    // TODO log every failure
    private static SemaphoreSlim _lock = new SemaphoreSlim(1);

    private readonly IPermissionsResolver _permissionsResolver;
    private readonly CustomVoiceService _customVoiceService;
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly IDiscordRestGuildAPI _guildApi;
    private readonly ILogger<CustomVoiceResponder> _logger;
    private readonly ICurrentPluginLifetime _lifetime;

    private readonly CustomVoiceOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomVoiceResponder"/> class.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="permissionsResolver">The permissions resolver.</param>
    /// <param name="customVoiceService">The custom voice service.</param>
    /// <param name="channelApi">The channel api.</param>
    /// <param name="guildApi">The guild api.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="lifetime">The lifetime.</param>
    public CustomVoiceResponder
    (
        IOptionsSnapshot<CustomVoiceOptions> options,
        IPermissionsResolver permissionsResolver,
        CustomVoiceService customVoiceService,
        IDiscordRestChannelAPI channelApi,
        IDiscordRestGuildAPI guildApi,
        ILogger<CustomVoiceResponder> logger,
        ICurrentPluginLifetime lifetime
    )
    {
        _permissionsResolver = permissionsResolver;
        _customVoiceService = customVoiceService;
        _channelApi = channelApi;
        _guildApi = guildApi;
        _logger = logger;
        _lifetime = lifetime;
        _options = options.Value;

    }

    /// <inheritdoc />
    public async Task<Result> RespondAsync(IVoiceStateUpdate gatewayEvent, CancellationToken ct = default)
    {
        var scheduleDeletion = _customVoiceService.UpdateMembers(gatewayEvent.UserID, gatewayEvent.ChannelID);
        if (scheduleDeletion)
        {
            var deleteResult = await ScheduleDeleteAsync(ct);
            if (!deleteResult.IsSuccess)
            {
                return deleteResult;
            }
        }

        if (gatewayEvent.ChannelID is null)
        {
            return Result.FromSuccess();
        }

        var channelId = gatewayEvent.ChannelID.Value;

        if (!gatewayEvent.GuildID.IsDefined(out var guildId))
        {
            return Result.FromSuccess();
        }

        try
        {
            // Lock to prevent generating multiple voice channels if the user is hopping between voice channels quickly.
            await _lock.WaitAsync(ct);
            return await HandlePossibleVoiceCreate(guildId, channelId, gatewayEvent.UserID, ct);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Checks if the joined channel is either stage or voice creation channel.
    /// If it is, proceeds to create custom voice/stage.
    /// </summary>
    /// <param name="guildId">The guild id.</param>
    /// <param name="channelId">The channel  id.</param>
    /// <param name="userId">The user id.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A result that may fail.</returns>
    private async Task<Result> HandlePossibleVoiceCreate(Snowflake guildId, Snowflake channelId, Snowflake userId, CancellationToken ct)
    {
        if (channelId.Value == _options.CreateStageChannelId || channelId.Value == _options.CreateVoiceChannelId)
        {
            // Already has a channel.
            var usersChannel = _customVoiceService.GetChannelUserIsConnectedTo(userId);
            if (usersChannel is not null)
            {
                _lock.Release();
                return await MoveMemberAsync
                (
                    guildId,
                    userId,
                    usersChannel.ChannelId,
                    "Unknown - reconnection",
                    ct
                );
            }

            // Cannot create custom channel.
            if (!await _permissionsResolver.HasPermissionAsync
                ("enhancements.customvoice.create", new DiscordTarget(userId, TargetType.User), ct))
            {
                _lock.Release();
                return Result.FromSuccess();
            }
        }

        if (channelId.Value == _options.CreateStageChannelId)
        {
            var result = await HandleCreateVoiceAsync
            (
                ChannelType.GuildStageVoice,
                guildId,
                channelId,
                userId,
                ct
            );
            return result;
        }

        if (channelId.Value == _options.CreateVoiceChannelId)
        {
            var result = await HandleCreateVoiceAsync
            (
                ChannelType.GuildVoice,
                guildId,
                channelId,
                userId,
                ct
            );
            return result;
        }
        return Result.FromSuccess();
    }

    /// <summary>
    /// Delete channels with no members.
    /// </summary>
    /// <remarks>
    /// Schedules deletion if RemoveAfterSecondsInactivity is greater than zero.
    /// </remarks>
    private async Task<Result> ScheduleDeleteAsync(CancellationToken ct)
    {
        foreach (var channelData in _customVoiceService.GetEmptyChannels())
        {
            if (_options.RemoveAfterSecondsInactivity == 0)
            {
                return await RemoveEmptyChannelAsync(channelData, ct);
            }

#pragma warning disable CS4014
            Task.Run
            (
                async () =>
                {
                    await Task.Delay(_options.RemoveAfterSecondsInactivity * 1000, _lifetime.Stopping);
                    if (channelData.Members.Data.Count == 0)
                    {
                        await RemoveEmptyChannelAsync(channelData, _lifetime.Stopping);
                    }
                }
            );
#pragma warning restore CS4014
        }

        return Result.FromSuccess();
    }

    /// <summary>
    /// Remove a channel and log it.
    /// </summary>
    /// <param name="channelData">The channel data to remove.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A result that may have failed.</returns>
    private async Task<Result> RemoveEmptyChannelAsync(CustomVoiceChannel channelData, CancellationToken ct)
    {
        var deleteResult = await _channelApi.DeleteChannelAsync
            (channelData.ChannelId, "Deleting empty Custom voice.", ct);
        _customVoiceService.RemoveVoice(channelData);

        if (deleteResult.IsSuccess)
        {
            _logger.LogInformation
                ("Deleted empty custom voice created by <@{Owner}>", channelData.OwnerId);
        }
        else
        {
            _logger.LogError
            (
                "Could not delete an empty voice channel <#{Channel}> created by <@{Owner}>",
                channelData.ChannelId,
                channelData.OwnerId
            );

        }

        return deleteResult;
    }

    /// <summary>
    /// Creates a custom voice or stage for the given user, moves the user to the voice channel.
    /// </summary>
    private async Task<Result> HandleCreateVoiceAsync
    (
        ChannelType type,
        Snowflake guildId,
        Snowflake channelId,
        Snowflake userId,
        CancellationToken ct
    )
    {
        var memberResult = await _guildApi.GetGuildMemberAsync(guildId, userId, ct);
        if (!memberResult.IsDefined(out var member))
        {
            return Result.FromError(memberResult);
        }

        if (!member.User.IsDefined(out var user))
        {
            return Result.FromSuccess();
        }

        var creationChannelResult = await _channelApi.GetChannelAsync(channelId, ct);
        if (!creationChannelResult.IsDefined(out var creationChannel))
        {
            return Result.FromError(creationChannelResult);
        }

        Optional<Snowflake> parentId = default;
        if (creationChannel.ParentID.HasValue)
        {
            parentId = creationChannel.ParentID.Value ?? default;
        }

        // Shorten the name so the channel has maximum of 100 characters in the name
        var nickname = member.Nickname.HasValue ? member.Nickname.Value ?? user.Username : user.Username;
        var name = _options.DefaultVoiceName
            .Replace
                ("{User}", nickname.Shorten(100 - _options.DefaultVoiceName.Replace("{User}", string.Empty).Length));

        // Create the voice
        var createdChannelResult = await CreateVoiceAsync
        (
            guildId,
            userId,
            name,
            type,
            parentId,
            ct
        );
        if (!createdChannelResult.IsDefined(out var createdChannel))
        {
            return Result.FromError(createdChannelResult);
        }

        // Move the user to the voice
        return await MoveMemberAsync
        (
            guildId,
            userId,
            createdChannel.ID,
            nickname,
            ct
        );
    }

    /// <summary>
    /// Creates the specified channel. Assigns permissions to the owner.
    /// </summary>
    private async Task<Result<IChannel>> CreateVoiceAsync
    (
        Snowflake guildId,
        Snowflake userId,
        string name,
        ChannelType type,
        Optional<Snowflake> parentId,
        CancellationToken ct
    )
    {
        var createdChannelResult = await _guildApi.CreateGuildChannelAsync
        (
            guildId,
            name,
            type,
            parentID: parentId,
            ct: ct
        );

        if (!createdChannelResult.IsDefined(out var createdChannel))
        {
            return Result<IChannel>.FromError(createdChannelResult);
        }

        _customVoiceService.AddVoice(createdChannel, userId);

        var permissionsEditResult = await _channelApi.EditChannelPermissionsAsync
        (
            createdChannel.ID,
            userId,
            new DiscordPermissionSet
            (
                DiscordPermission.MoveMembers,
                DiscordPermission.MuteMembers,
                DiscordPermission.DeafenMembers,
                DiscordPermission.ManageChannels
            ),
            DiscordPermissionSet.Empty,
            PermissionOverwriteType.Member,
            "Custom voice channel owner permissions assignment",
            ct
        );

        if (!permissionsEditResult.IsSuccess)
        {
            return Result<IChannel>.FromError(permissionsEditResult);
        }

        return Result<IChannel>.FromSuccess(createdChannel);
    }

    /// <summary>
    /// Moves the member to the specified voice channel.
    /// </summary>
    private async Task<Result> MoveMemberAsync
    (
        Snowflake guildId,
        Snowflake userId,
        Snowflake channelId,
        string nickname,
        CancellationToken ct
    )
    {
        var modifyResult = await _guildApi.ModifyGuildMemberAsync
        (
            guildId,
            userId,
            channelID: channelId,
            reason: "Move to custom voice channel",
            ct: ct
        );

        if (!modifyResult.IsSuccess)
        {
            _logger.LogError
                ("Created a voice channel for <@{User}> ({Nickname}), but could not move him there.", userId, nickname);
            return modifyResult;
        }

        _logger.LogInformation
        (
            "Created a temporary voice channel <#{Channel}> for <@{User}> ({Nickname})",
            channelId,
            userId,
            nickname
        );

        return modifyResult;
    }
}