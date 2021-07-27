using Christofel.BaseLib.Database.Models.Enums;
using Discord;
using Microsoft.EntityFrameworkCore;

namespace Christofel.BaseLib.Database.Models
{
    [Owned]
    public class DiscordTarget
    {
        public static DiscordTarget Everyone => new DiscordTarget(0, TargetType.Everyone);

        public DiscordTarget() {}
        
        public DiscordTarget(ulong discordId, TargetType type)
        {
            DiscordId = discordId;
            TargetType = type;
        }
        
        public ulong DiscordId { get; set; }
        
        public ulong? GuildId { get; set; }

        public TargetType TargetType { get; set; }
    }
}