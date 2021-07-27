using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;
using Christofel.BaseLib.Permissions;
using Discord;

namespace Christofel.CommandsLib.Extensions
{
    public static class IPermissionResolverExtensions
    {
        public static async Task<ApplicationCommandPermission[]> GetSlashCommandPermissionsAsync(this IPermissionsResolver resolver, IPermission permission)
        { 
            IEnumerable<DiscordTarget> allowedDiscordTargets = await resolver
                .GetPermissionTargetsAsync(permission);

            return allowedDiscordTargets
                .Where(x => x.TargetType != TargetType.Everyone)
                .Select(x => new ApplicationCommandPermission(x.DiscordId, x.TargetType.AsApplicationCommandPermission(), true))
                .ToArray();
        }
    }
}