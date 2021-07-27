using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;
using Discord;

namespace Christofel.BaseLib.Extensions
{
    public static class IRoleExtensions
    {
        public static DiscordTarget ToDiscordTarget(this IRole role)
        {
            return new DiscordTarget
            {
                DiscordId = role.Id,
                TargetType = TargetType.Role
            };
        }
    }
}