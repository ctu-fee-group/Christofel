//
//   ApplicationCommandPermissionsExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Discord.API.Abstractions.Objects;

namespace Christofel.CommandsLib.Extensions
{
    public static class ApplicationCommandPermissionsExtensions
    {
        public static bool PermissionMatches
            (this IApplicationCommandPermissions leftPermission, IApplicationCommandPermissions rightPermission)
            => leftPermission.ID == rightPermission.ID &&
               leftPermission.HasPermission == rightPermission.HasPermission &&
               leftPermission.Type == rightPermission.Type;
    }
}