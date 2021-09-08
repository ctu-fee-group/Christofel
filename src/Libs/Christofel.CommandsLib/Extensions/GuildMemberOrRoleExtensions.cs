using System;
using System.Collections.Generic;
using System.Linq;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;
using Christofel.BaseLib.Extensions;
using OneOf;
using Remora.Discord.API.Abstractions.Objects;

namespace Christofel.CommandsLib.Extensions
{
    public static class GuildMemberOrRoleExtensions
    {
        public static DiscordTarget ToDiscordTarget(this OneOf<IPartialGuildMember, IRole> memberOrRole)
        {
            if (memberOrRole.IsT0)
            {
                var guildMember = memberOrRole.AsT0;
                var user = guildMember.User;

                if (!user.HasValue)
                {
                    throw new ArgumentException("GuildMember User must be set");
                }

                return new DiscordTarget(user.Value.ID, TargetType.User);
            }

            if (memberOrRole.IsT1)
            {
                var role = memberOrRole.AsT1;

                if (role.Name == "@everyone")
                {
                    return DiscordTarget.Everyone;
                }

                return new DiscordTarget(role.ID, TargetType.Role);
            }

            throw new InvalidOperationException("Nor User, nor role is set");
        }

        public static IEnumerable<DiscordTarget> GetAllDiscordTargets(
            this OneOf<IPartialGuildMember, IRole> memberOrRole)
        {
            if (memberOrRole.IsT0)
            {
                var member = memberOrRole.AsT0;
                var targets = member.GetAllDiscordTargets().ToList();
                
                targets.Add(DiscordTarget.Everyone);
                return targets;
            }

            if (memberOrRole.IsT1)
            {
                var targets = new List<DiscordTarget>();
                var role = memberOrRole.AsT1; 
                
                if (role.Name == "@everyone")
                {
                    targets.Add(DiscordTarget.Everyone);
                }
                else
                {
                    targets.Add(new DiscordTarget(role.ID, TargetType.Role));
                }

                return targets;
            }
            
            throw new InvalidOperationException("Parsing of member or role failed");
        }
    }
}