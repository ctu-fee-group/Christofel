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
public record CustomVoiceChannel
(
    Snowflake GuildId,
    Snowflake ChannelId,
    Snowflake OwnerId,
    IThreadSafeStorage<Snowflake> Members,
    DateTime CreationTime
);