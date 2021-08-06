using System.Collections.Generic;
using System.Linq;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;
using Discord;

namespace Christofel.BaseLib.Extensions
{
    public static class IUserExtensions
    {
        /// <summary>
        /// Converts discord user to DiscordTarget for better use with permissions
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public static DiscordTarget ToDiscordTarget(this IUser role)
        {
            return new DiscordTarget
            {
                DiscordId = role.Id,
                TargetType = TargetType.User
            };
        }

        public static IEnumerable<DiscordTarget> GetAllDiscordTargets(this IUser user)
        {
            List<DiscordTarget> targets = new List<DiscordTarget>();
            
            if (user is IGuildUser guildUser)
            {
                targets.AddRange(guildUser.RoleIds.Select(x => new DiscordTarget(x, TargetType.Role)));
            }
            
            targets.Add(DiscordTarget.Everyone);
            targets.Add(user.ToDiscordTarget());

            return targets;
        }
    }
}