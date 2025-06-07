//
//   DiscordLogMessage.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Logger
{
    /// <summary>
    /// Message to be logged into Discord.
    /// </summary>
    /// <param name="GuildId">The id of the guild to send the message to.</param>
    /// <param name="ChannelId">The id of the channel to send the message to.</param>
    /// <param name="Message">The message to send.</param>
    public record DiscordLogMessage(ulong GuildId, ulong ChannelId, string Message);
}