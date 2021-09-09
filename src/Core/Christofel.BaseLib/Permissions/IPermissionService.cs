//
//   IPermissionService.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Christofel.BaseLib.Permissions
{
    /// <summary>
    ///     Service for registering permissions during runtime.
    ///     Resolver is exposed as well.
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        ///     Resolver for resolving application permissions
        /// </summary>
        public IPermissionsResolver Resolver { get; }

        /// <summary>
        ///     Enumerable of all permissions that are registered
        /// </summary>
        public IEnumerable<IPermission> Permissions { get; }

        /// <summary>
        ///     Store permission
        /// </summary>
        /// <param name="permission">What permission to store</param>
        public void RegisterPermission(IPermission permission);

        /// <summary>
        ///     Remove stored permission
        /// </summary>
        /// <param name="permission">What permission to remove</param>
        public void UnregisterPermission(IPermission permission);

        /// <summary>
        ///     Removes all perimssions mathing the specified name
        /// </summary>
        /// <remarks>
        ///     No wildcards are enabled, only the full name
        /// </remarks>
        /// <param name="permissionName">What permissions to remove</param>
        public void UnregisterPermission(string permissionName);
    }
}