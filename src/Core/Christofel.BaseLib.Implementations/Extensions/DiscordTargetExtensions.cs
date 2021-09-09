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
    public static class DiscordTargetExtensions
    {
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