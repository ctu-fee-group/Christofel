//
//   CustomVoiceOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Rest.Core;

namespace Christofel.Enhancements.CustomVoice;

/// <summary>
/// Options for <see cref="CustomVoiceResponder"/>.
/// </summary>
public class CustomVoiceOptions
{
    /// <summary>
    /// Gets or sets the maximal number of channels that can be created.
    /// </summary>
    public ushort MaxChannels { get; set; } = 10;

    /// <summary>
    /// Gets or sets the id of the channel to create stage channel with.
    /// </summary>
    public ulong? CreateStageChannelId { get; set; }

    /// <summary>
    /// Gets or sets the id of the channel to create voice channel with.
    /// </summary>
    public ulong? CreateVoiceChannelId { get; set; }

    /// <summary>
    /// Gets or sets the amount of seconds to remove the voice/stage channel after there are no members joined.
    /// </summary>
    public ushort RemoveAfterSecondsInactivity { get; set; } = 10;

    /// <summary>
    /// Gets or sets the default name of the voice channel to be created.
    /// </summary>
    public string DefaultVoiceName { get; set; } = "{User}'s voice";
}