//
//   PermissionResolverExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Permissions;

namespace Christofel.BaseLib.Extensions
{
    public static class PermissionResolverExtensions
    {
        /// <summary>
        /// Alias for <see cref="IPermissionsResolver.GetPermissionTargetsAsync" />
        /// using permission object
        /// </summary>
        /// <param name="resolver"></param>
        /// <param name="permission"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Task<IEnumerable<DiscordTarget>> GetPermissionTargetsAsync
        (
            this IPermissionsResolver resolver,
            IPermission permission,
            CancellationToken token = default
        ) => resolver.GetPermissionTargetsAsync(permission.PermissionName, token);

        /// <summary>
        /// Alias for <see cref="IPermissionsResolver.HasPermissionAsync" />
        /// using permission object
        /// </summary>
        /// <param name="resolver"></param>
        /// <param name="permission"></param>
        /// <param name="target"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Task<bool> HasPermissionAsync
        (
            this IPermissionsResolver resolver,
            IPermission permission,
            DiscordTarget target,
            CancellationToken token = default
        ) => resolver.HasPermissionAsync(permission.PermissionName, target, token);

        /// <summary>
        /// Alias for <see cref="IPermissionsResolver.AnyHasPermissionAsync" />
        /// using permission object
        /// </summary>
        /// <param name="resolver"></param>
        /// <param name="permission"></param>
        /// <param name="target"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Task<bool> AnyHasPermissionAsync
        (
            this IPermissionsResolver resolver,
            IPermission permission,
            IEnumerable<DiscordTarget> target,
            CancellationToken token = default
        ) => resolver.AnyHasPermissionAsync(permission.PermissionName, target, token);
    }
}