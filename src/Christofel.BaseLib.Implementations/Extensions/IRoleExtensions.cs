using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;
using Discord;

namespace Christofel.BaseLib.Extensions
{
    public static class IRoleExtensions
    {
        /// <summary>
        /// Converts discord role to DiscordTarget for better use with permissions
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
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