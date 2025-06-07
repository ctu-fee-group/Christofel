//
//   IPermissionResolverExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Common.Database.Models;
using Christofel.Common.Database.Models.Enums;
using Christofel.Common.Permissions;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;

namespace Christofel.CommandsLib.Extensions
{
    /// <summary>
    /// Class containing extensions for <see cref="IPermissionsResolver"/>.
    /// </summary>
    public static class IPermissionResolverExtensions
    {
        /// <summary>
        /// Get assigned permissions for a slash command.
        /// </summary>
        /// <param name="resolver">The resolver to use.</param>
        /// <param name="permission">The permission that represents the permission of the command.</param>
        /// <param name="token">The cancellation token for the operation.</param>
        /// <returns>Permissions that should be assigned to the slash command.</returns>
        public static async Task<IEnumerable<IApplicationCommandPermissions>> GetSlashCommandPermissionsAsync
            (this IPermissionsResolver resolver, string permission, CancellationToken token = default)
        {
            IEnumerable<DiscordTarget> allowedDiscordTargets = await resolver
                .GetPermissionTargetsAsync(permission, token);

            return allowedDiscordTargets
                .Where(x => x.TargetType != TargetType.Everyone)
                .Select
                (
                    x =>
                        new ApplicationCommandPermissions
                            (x.DiscordId, x.TargetType.AsApplicationCommandPermission(), true)
                );
        }
    }
}