using System;
using System.Collections.Generic;
using System.Linq;
using Discord;

namespace Christofel.CommandsLib.Extensions
{
    public static class GuildApplicationCommandPermissionExtensions
    {
        public static bool MatchesPermissions(this GuildApplicationCommandPermission commandPermission,
            ApplicationCommandPermission[] permissions)
        {
            return Enumerable.SequenceEqual<ApplicationCommandPermission>(
                commandPermission.Permissions.OrderBy(x => x),
                permissions.OrderBy(x => x),
                new ApplicationCommandPermissionComparer());
        }

        private class ApplicationCommandPermissionComparer : IEqualityComparer<ApplicationCommandPermission>
        {
            public bool Equals(ApplicationCommandPermission? x, ApplicationCommandPermission? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.TargetId == y.TargetId && x.TargetType == y.TargetType && x.Permission == y.Permission;
            }

            public int GetHashCode(ApplicationCommandPermission obj)
            {
                return HashCode.Combine(obj.TargetId, (int) obj.TargetType, obj.Permission);
            }
        }
    }
}