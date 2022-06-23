//
//   CustomVoiceService.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.CommandsLib.Extensions;
using Christofel.Common.Database.Models;
using Christofel.Common.Database.Models.Enums;
using Christofel.Common.Permissions;
using Christofel.Helpers.Permissions;
using Christofel.Helpers.Storages;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.Enhancements.CustomVoice;

/// <summary>
/// The service for custom voices.
/// </summary>
public class CustomVoiceService
{
    private readonly IThreadSafeStorage<CustomVoiceChannel> _customVoicesStorage;
    private readonly MemberPermissionResolver _permissionsResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomVoiceService"/> class.
    /// </summary>
    /// <param name="customVoicesStorage">The custom voices storage.</param>
    /// <param name="permissionsResolver">The permissions resolver.</param>
    public CustomVoiceService
        (IThreadSafeStorage<CustomVoiceChannel> customVoicesStorage, MemberPermissionResolver permissionsResolver)
    {
        _customVoicesStorage = customVoicesStorage;
        _permissionsResolver = permissionsResolver;
    }

    /// <summary>
    /// Gets the custom voice channel the user is in. Null if in none.
    /// </summary>
    /// <remarks>
    /// If the user is in non-custom voice channel, null will be returned.
    /// </remarks>
    /// <param name="userId">The user to try to find.</param>
    /// <returns>The custom voice the user is in, if any.</returns>
    public CustomVoiceChannel? GetChannelUserIsConnectedTo(Snowflake userId)
    {
        foreach (var customVoice in _customVoicesStorage.Data)
        {
            if (customVoice.Members.Data.Contains(userId))
            {
                return customVoice;
            }
        }

        return null;
    }

    /// <summary>
    /// Removes the given voice from the storage.
    /// </summary>
    /// <param name="customVoiceChannel">The custom voice to remove.</param>
    public void RemoveVoice(CustomVoiceChannel customVoiceChannel)
    {
        _customVoicesStorage.Remove(customVoiceChannel);
    }

    /// <summary>
    /// Adds the given voice to the storage.
    /// </summary>
    /// <param name="customVoiceChannel">The custom voice to add.</param>
    public void AddVoice(CustomVoiceChannel customVoiceChannel)
    {
        _customVoicesStorage.Add(customVoiceChannel);
    }

    /// <summary>
    /// Adds the given voice to the storage.
    /// </summary>
    /// <param name="customVoice">The custom voice channel.</param>
    /// <param name="ownerId">The id of the owner user.</param>
    /// <returns>The result that may have failed.</returns>
    public Result AddVoice(IChannel customVoice, Snowflake ownerId)
    {
        if (!customVoice.GuildID.IsDefined(out var guildId))
        {
            return new ArgumentInvalidError("customVoice", "The custom voice cannot have empty guild id.");
        }

        AddVoice
        (
            new CustomVoiceChannel
            (
                guildId,
                customVoice.ID,
                ownerId,
                new ThreadSafeListStorage<Snowflake>(),
                DateTime.Now
            )
        );
        return Result.FromSuccess();
    }

    /// <summary>
    /// Receives what user has joined or left a voice channel and updates the stored custom voice members.
    /// </summary>
    /// <remarks>
    /// Adds the member to the custom voice he has connected to.
    /// Removes the member from all other voice channels.
    /// </remarks>
    /// <param name="userId">The user that has updated state.</param>
    /// <param name="currentChannelId">The channel id the user has connected to.</param>
    /// <returns>Whether there is an empty channel.</returns>
    public bool UpdateMembers(Snowflake userId, Snowflake? currentChannelId)
    {
        bool scheduleDelete = false;
        foreach (var channel in _customVoicesStorage.Data)
        {
            if (channel.ChannelId == currentChannelId)
            {
                if (!channel.Members.Data.Contains(userId))
                {
                    channel.Members.Add(userId);
                }
            }
            else
            {
                channel.Members.Remove(userId);

                if (channel.Members.Data.Count == 0)
                {
                    scheduleDelete = true;
                }
            }
        }

        return scheduleDelete;
    }

    /// <summary>
    /// Returns the channels with zero members.
    /// </summary>
    /// <returns>The channels with zero members, if any.</returns>
    public IEnumerable<CustomVoiceChannel> GetEmptyChannels()
    {
        return _customVoicesStorage
            .Data
            .Where(x => x.Members.Data.Count == 0);
    }

    /// <summary>
    /// Checks whether the given user is permitted to control the channel.
    /// </summary>
    /// <param name="userId">The id of the user.</param>
    /// <param name="channel">The custom voice.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>Whether the user has permission to control the channel.</returns>
    public async Task<Result<bool>> IsPermittedToChangeChannel
        (Snowflake userId, CustomVoiceChannel channel, CancellationToken ct)
    {
        if (channel.OwnerId == userId)
        {
            return true;
        }

        return await _permissionsResolver.HasPermissionAsync
            ("enhancements.customvoice.override", userId, channel.GuildId, ct: ct);
    }
}