using System;
using System.Collections.Generic;
using System.Linq;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;
using Christofel.BaseLib.Extensions;
using Christofel.CommandsLib.ContextedParsers;

namespace Christofel.CommandsLib.Extensions
{
    public static class GuildMemberOrRoleExtensions
    {
        public static DiscordTarget ToDiscordTarget(this IGuildMemberOrRole memberOrRole)
        {
            if (memberOrRole.User is not null)
            {
                return new DiscordTarget(memberOrRole.User.ID, TargetType.User);
            }

            if (memberOrRole.Role is not null)
            {
                if (memberOrRole.Role.Name == "@everyone")
                {
                    return DiscordTarget.Everyone;
                }
                else
                {
                    return new DiscordTarget(memberOrRole.Role.ID, TargetType.Role);
                }
            }

            throw new InvalidOperationException("Nor User, nor role is set");
        }

        public static IEnumerable<DiscordTarget> GetAllDiscordTargets(this IGuildMemberOrRole memberOrRole)
        {
            var targets = memberOrRole.Member?.GetAllDiscordTargets().ToList();

            if (targets is null && memberOrRole.Role is not null)
            {
                targets = new List<DiscordTarget>();
                if (memberOrRole.Role.Name == "@everyone")
                {
                    targets.Add(DiscordTarget.Everyone);
                }
                else
                {
                    targets.Add(new DiscordTarget(memberOrRole.Role.ID, TargetType.Role));
                }
            }
            else if (targets is null)
            {
                throw new InvalidOperationException("Parsing of member or role failed");
            }

            if (memberOrRole.User is not null)
            {
                targets.Add(DiscordTarget.Everyone);
                targets.Add(new DiscordTarget(memberOrRole.User.ID, TargetType.User));
            }

            return targets;
        }
    }
}