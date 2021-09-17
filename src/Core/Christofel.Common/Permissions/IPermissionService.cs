//
//   IPermissionService.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Christofel.Common.Permissions
{
    /// <summary>
    /// Service for registering permissions during runtime.
    /// Resolver is exposed as well.
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// Gets resolver for resolving application permissions.
        /// </summary>
        public IPermissionsResolver Resolver { get; }

        /// <summary>
        /// Gets enumerable of all permissions that are registered.
        /// </summary>
        public IEnumerable<IPermission> Permissions { get; }

        /// <summary>
        /// Registers permission into the storage.
        /// </summary>
        /// <param name="permission">What permission to store.</param>
        public void RegisterPermission(IPermission permission);

        /// <summary>
        /// Removes stored permission from the storage.
        /// </summary>
        /// <param name="permission">What permission to remove.</param>
        public void UnregisterPermission(IPermission permission);

        /// <summary>
        /// Removes all permissions matching the specified name.
        /// </summary>
        /// <remarks>
        /// Full name must be specified, wildcards are not supported.
        /// </remarks>
        /// <param name="permissionName">What permissions to remove.</param>
        public void UnregisterPermission(string permissionName);
    }
}