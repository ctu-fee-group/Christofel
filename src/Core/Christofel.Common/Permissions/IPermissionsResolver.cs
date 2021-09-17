//
//   IPermissionsResolver.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Common.Database.Models;

namespace Christofel.Common.Permissions
{
    /// <summary>
    /// Service for resolving permissions of users and roles.
    /// </summary>
    /// <remarks>
    /// Dot notation permissions are supported.
    /// Wildcards may be used for specifying multiple permissions.
    /// Permission `a.*` will grant the user permissions like `a.a`, `a.b`, `a.a.a`.
    /// </remarks>
    public interface IPermissionsResolver
    {
        /// <summary>
        /// Gets what targets have permission with the given name.
        /// </summary>
        /// <param name="permissionName">Name of the permission.</param>
        /// <param name="token">The cancellation token for the operation.</param>
        /// <returns>Collection containing all DiscordTargets that have the permission assigned.</returns>
        public Task<IEnumerable<DiscordTarget>> GetPermissionTargetsAsync
            (string permissionName, CancellationToken token = default);

        /// <summary>
        /// Checks whether the specified target has the given permission.
        /// </summary>
        /// <param name="permissionName">The name of the permission to be checked.</param>
        /// <param name="target">The target to be checked.</param>
        /// <param name="token">The cancellation token for the operation.</param>
        /// <returns>Whether the target has the permission.</returns>
        public Task<bool> HasPermissionAsync
            (string permissionName, DiscordTarget target, CancellationToken token = default);

        /// <summary>
        /// Checks whether any of the specified targets has the permission needed.
        /// </summary>
        /// <param name="permissionName">The name of the permission to be checked.</param>
        /// <param name="targets">The targets that .</param>
        /// <param name="token">The cancellation token for the operation.</param>
        /// <returns>Whether any of the targets has the permission.</returns>
        public Task<bool> AnyHasPermissionAsync
        (
            string permissionName,
            IEnumerable<DiscordTarget> targets,
            CancellationToken token = default
        );
    }
}