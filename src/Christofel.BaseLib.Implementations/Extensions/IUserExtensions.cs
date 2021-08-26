using System;
using System.Collections.Generic;
using System.Linq;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;
using Remora.Discord.API.Abstractions.Objects;

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
                DiscordId = role.ID.Value,
                TargetType = TargetType.User
            };
        }

        public static IEnumerable<DiscordTarget> GetAllDiscordTargets(this IGuildMember guildMember)
        {
            List<DiscordTarget> targets = new List<DiscordTarget>();

            targets.AddRange(guildMember.Roles.Select(x => new DiscordTarget(x.Value, TargetType.Role)));
            targets.Add(DiscordTarget.Everyone);
            
            if (guildMember.User.HasValue)
            {
                targets.Add(guildMember.User.Value.ToDiscordTarget());
            }
            else
            {
                throw new InvalidOperationException("Will get to this");
            }

            return targets;
        }
    }
}