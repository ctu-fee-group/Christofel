//
//   DiscordTargetExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;

namespace Christofel.BaseLib.Extensions
{
    /// <summary>
    /// Class containing extensions for <see cref="DiscordTarget"/>.
    /// </summary>
    public static class DiscordTargetExtensions
    {
        /// <summary>
        /// gets mention string for the given <see cref="DiscordTarget"/>.
        /// </summary>
        /// <param name="target">The target to get mention string of.</param>
        /// <returns>Mention string representing the <see cref="DiscordTarget"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the target is invalid type.</exception>
        public static string GetMentionString(this DiscordTarget target) =>
            target.TargetType switch
            {
                TargetType.Everyone => "@everyone",
                TargetType.Role => $@"<@&{target.DiscordId}>",
                TargetType.User => $@"<@{target.DiscordId}>",
                _ => throw new ArgumentOutOfRangeException(),
            };
    }
}