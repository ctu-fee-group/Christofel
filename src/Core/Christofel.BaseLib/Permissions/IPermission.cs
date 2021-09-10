//
//   IPermission.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.BaseLib.Permissions
{
    /// <summary>
    /// Permission information that can be used in lists of permissions.
    /// </summary>
    public interface IPermission
    {
        /// <summary>
        /// Gets name of the permission in dot notation.
        /// </summary>
        public string PermissionName { get; }

        /// <summary>
        /// Gets display name of the permission.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets short description of the permission.
        /// </summary>
        public string Description { get; }
    }
}