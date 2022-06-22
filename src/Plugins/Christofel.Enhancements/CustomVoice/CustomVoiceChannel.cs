//
//   CustomVoiceChannel.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Helpers.Storages;
using Remora.Rest.Core;

namespace Christofel.Enhancements.CustomVoice;

/// <summary>
/// Information about temporary created custom voice channel.
/// </summary>
/// <param name="GuildId"></param>
/// <param name="ChannelId"></param>
/// <param name="OwnerId"></param>
public record CustomVoiceChannel
(
    Snowflake GuildId,
    Snowflake ChannelId,
    Snowflake OwnerId,
    DateTime CreationTime,
    IThreadSafeStorage<Snowflake> Members
);