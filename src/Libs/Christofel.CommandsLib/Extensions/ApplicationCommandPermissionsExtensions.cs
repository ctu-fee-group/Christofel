//
//   ApplicationCommandPermissionsExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Discord.API.Abstractions.Objects;

namespace Christofel.CommandsLib.Extensions
{
    /// <summary>
    /// Class containing extensions for <see cref="IApplicationCommandPermissions"/>.
    /// </summary>
    public static class ApplicationCommandPermissionsExtensions
    {
        /// <summary>
        /// Checks if the <paramref name="leftPermission"/> matches <paramref name="rightPermission"/>.
        /// </summary>
        /// <param name="leftPermission">The permission to be matched against <paramref name="rightPermission"/>.</param>
        /// <param name="rightPermission">The permission to be matched against <paramref name="leftPermission"/>.</param>
        /// <returns>Whether <paramref name="leftPermission"/> matches <paramref name="rightPermission"/>.</returns>
        public static bool PermissionMatches
            (this IApplicationCommandPermissions leftPermission, IApplicationCommandPermissions rightPermission)
            => leftPermission.ID == rightPermission.ID &&
               leftPermission.HasPermission == rightPermission.HasPermission &&
               leftPermission.Type == rightPermission.Type;
    }
}