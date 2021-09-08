using System.Collections.Generic;
using System.Linq;
using Remora.Discord.API.Abstractions.Objects;

namespace Christofel.CommandsLib.Extensions
{
    public static class ApplicationCommandPermissionsExtensions
    {
        public static bool PermissionMatches(this IApplicationCommandPermissions leftPermission, IApplicationCommandPermissions rightPermission)
        {
            return (leftPermission.ID == rightPermission.ID &&
                    leftPermission.HasPermission == rightPermission.HasPermission &&
                    leftPermission.Type == rightPermission.Type);
        }
    }
}