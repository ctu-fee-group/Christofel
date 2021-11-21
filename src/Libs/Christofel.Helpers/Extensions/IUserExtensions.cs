//
//   IUserExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using Christofel.Common.Database.Models;
using Christofel.Common.Database.Models.Enums;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Rest.Core;

namespace Christofel.BaseLib.Extensions
{
    /// <summary>
    /// Class containing extensions for <see cref="IUser"/>.
    /// </summary>
    public static class IUserExtensions
    {
        /// <summary>
        /// Converts discord user to DiscordTarget for better use with permissions.
        /// </summary>
        /// <param name="user">The user to be converted.</param>
        /// <returns>Converted <see cref="DiscordTarget"/>.</returns>
        public static DiscordTarget ToDiscordTarget(this IUser user) => new DiscordTarget(user.ID, TargetType.User);

        /// <summary>
        /// Gets all discord targets of the specified member.
        /// </summary>
        /// <param name="guildMember">The member to get targets of.</param>
        /// <returns>All of the targets that are associated with the guild member.</returns>
        public static IEnumerable<DiscordTarget> GetAllDiscordTargets
            (this IGuildMember guildMember) => GetAllDiscordTargets(guildMember.Roles, guildMember.User);

        /// <summary>
        /// Gets all discord targets of the specified member.
        /// </summary>
        /// <param name="guildMember">The member to get targets of.</param>
        /// <returns>All of the targets that are associated with the guild member.</returns>
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