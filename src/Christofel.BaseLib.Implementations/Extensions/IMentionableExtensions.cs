using System;
using Christofel.BaseLib.Database.Models;
using Discord;

namespace Christofel.BaseLib.Extensions
{
    public static class IMentionableExtensions
    {
        public static DiscordTarget ToDiscordTarget(this IMentionable mentionable)
        {
            DiscordTarget target;
            if (mentionable is IUser user)
            {
                target = user.ToDiscordTarget();
            }
            else if (mentionable is IRole role)
            {
                target = role.ToDiscordTarget();
            }
            else
            {
                throw new InvalidOperationException(
                    $@"Mentionable {mentionable} is nor user nor role. Cannot determine DiscordTarget");
            }

            return target;
        }
    }
}