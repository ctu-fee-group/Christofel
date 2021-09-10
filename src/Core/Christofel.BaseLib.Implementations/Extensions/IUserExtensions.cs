//
//   IUserExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Core;

namespace Christofel.BaseLib.Extensions
{
    public static class IUserExtensions
    {
        /// <summary>
        /// Converts discord user to DiscordTarget for better use with permissions
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public static DiscordTarget ToDiscordTarget(this IUser role) => new DiscordTarget
        {
            DiscordId = role.ID, TargetType = TargetType.User,
        };

        public static IEnumerable<DiscordTarget> GetAllDiscordTargets
            (this IGuildMember guildMember) => GetAllDiscordTargets(guildMember.Roles, guildMember.User);

        public static IEnumerable<DiscordTarget> GetAllDiscordTargets
            (this IPartialGuildMember guildMember) => GetAllDiscordTargets(guildMember.Roles.Value, guildMember.User);

        private static IEnumerable<DiscordTarget> GetAllDiscordTargets
        (
            IEnumerable<Snowflake> roles,
            Optional<IUser> userOptional
        )
        {
            List<DiscordTarget> targets = new List<DiscordTarget>();

            targets.AddRange(roles.Select(x => new DiscordTarget(x, TargetType.Role)));
            targets.Add(DiscordTarget.Everyone);

            if (userOptional.IsDefined(out var user))
            {
                targets.Add(user.ToDiscordTarget());
            }

            return targets;
        }
    }
}