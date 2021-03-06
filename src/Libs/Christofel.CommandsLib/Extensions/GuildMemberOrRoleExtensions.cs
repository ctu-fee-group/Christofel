//
//   GuildMemberOrRoleExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Christofel.BaseLib.Extensions;
using Christofel.Common.Database.Models;
using Christofel.Common.Database.Models.Enums;
using OneOf;
using Remora.Discord.API.Abstractions.Objects;

namespace Christofel.CommandsLib.Extensions
{
    /// <summary>
    /// Class containing extensions for converting members or roles to DiscordTarget.
    /// </summary>
    public static class GuildMemberOrRoleExtensions
    {
        /// <summary>
        /// Creates <see cref="DiscordTarget"/> out of <see name="memberOrRole"/>.
        /// </summary>
        /// <param name="memberOrRole">The member or role to be converted.</param>
        /// <param name="treatEveryoneAsRole">Whether to return everyone as a role instead of TargetType Everyone.</param>
        /// <returns>The target representing <paramref name="memberOrRole"/>.</returns>
        /// <exception cref="ArgumentException">Thrown if the user does not have a value.</exception>
        /// <exception cref="InvalidOperationException">Thrown if nor user, nor role is set.</exception>
        public static DiscordTarget ToDiscordTarget
            (this OneOf<IPartialGuildMember, IRole> memberOrRole, bool treatEveryoneAsRole = false)
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

                if (role.Name == "@everyone" && !treatEveryoneAsRole)
                {
                    return DiscordTarget.Everyone;
                }

                return new DiscordTarget(role.ID, TargetType.Role);
            }

            throw new InvalidOperationException("Nor User, nor role is set");
        }

        /// <summary>
        /// Creates <see cref="DiscordTarget"/> of the given <paramref name="memberOrRole"/>.
        /// </summary>
        /// <remarks>
        /// For role, returns only the role.
        ///
        /// For user, returns the user and each his role with everyone as well.
        /// </remarks>
        /// <param name="memberOrRole">The member or role to be converted.</param>
        /// <returns>All <see cref="DiscordTarget"/>s that represent <paramref name="memberOrRole"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the parsing has failed.</exception>
        public static IEnumerable<DiscordTarget> GetAllDiscordTargets
        (
            this OneOf<IPartialGuildMember, IRole> memberOrRole
        )
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