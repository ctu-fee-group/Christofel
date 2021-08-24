using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;
using Christofel.BaseLib.Permissions;

namespace Christofel.CommandsLib.Extensions
{
    public static class IPermissionResolverExtensions
    {
        /// <summary>
        /// Get assigned permissions for a slash command
        /// </summary>
        /// <param name="resolver"></param>
        /// <param name="permission"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<ApplicationCommandPermission>> GetSlashCommandPermissionsAsync(this IPermissionsResolver resolver, string permission, CancellationToken token = new CancellationToken())
        {
            IEnumerable<DiscordTarget> allowedDiscordTargets = await resolver
                .GetPermissionTargetsAsync(permission, token);

            return allowedDiscordTargets
                .Where(x => x.TargetType != TargetType.Everyone)
                .Select(x =>
                    new ApplicationCommandPermission(x.DiscordId, x.TargetType.AsApplicationCommandPermission(), true));
        }
    }
}