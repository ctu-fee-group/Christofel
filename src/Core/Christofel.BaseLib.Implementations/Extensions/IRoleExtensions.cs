//
//   IRoleExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;
using Remora.Discord.API.Abstractions.Objects;

namespace Christofel.BaseLib.Extensions
{
    public static class IRoleExtensions
    {
        /// <summary>
        ///     Converts discord role to DiscordTarget for better use with permissions
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public static DiscordTarget ToDiscordTarget(this IRole role)
        {
            if (role.Name == "@everyone")
            {
                return new DiscordTarget { TargetType = TargetType.Everyone };
            }

            return new DiscordTarget { DiscordId = role.ID, TargetType = TargetType.Role };
        }
    }
}