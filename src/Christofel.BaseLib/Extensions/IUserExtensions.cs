using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;
using Discord;

namespace Christofel.BaseLib.Extensions
{
    public static class IUserExtensions
    {
        public static DiscordTarget ToDiscordTarget(this IUser role)
        {
            return new DiscordTarget
            {
                DiscordId = role.Id,
                TargetType = TargetType.User
            };
        }
    }
}