using System;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;

namespace Christofel.BaseLib.Extensions
{
    public static class DiscordTargetExtensions
    {
        public static string GetMentionString(this DiscordTarget target) =>
            target.TargetType switch
            {
                TargetType.Everyone => "@everyone",
                TargetType.Role => $@"<@&{target.DiscordId}>",
                TargetType.User => $@"<@{target.DiscordId}>",
                _ => throw new ArgumentOutOfRangeException()
            };
    }
}