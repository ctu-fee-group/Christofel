//
//   DiscordLogMessage.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Logger
{
    public class DiscordLogMessage
    {
        public DiscordLogMessage(ulong guildId, ulong channelId, string message)
        {
            GuildId = guildId;
            ChannelId = channelId;
            Message = message;
        }

        public ulong GuildId { get; set; }

        public ulong ChannelId { get; set; }

        public string Message { get; set; }
    }
}