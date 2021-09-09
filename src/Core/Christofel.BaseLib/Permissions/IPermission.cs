//
//   IPermission.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.BaseLib.Permissions
{
    /// <summary>
    ///     Permission information that can be used in lists of permissions
    /// </summary>
    public interface IPermission
    {
        /// <summary>
        ///     Name in dot notation
        /// </summary>
        public string PermissionName { get; }

        /// <summary>
        ///     Display name for better orientation of what the permission does
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        ///     Short description saying what this permission is used for
        /// </summary>
        public string Description { get; }
    }
}