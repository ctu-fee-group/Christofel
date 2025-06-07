//
//   RequirePermissionAttribute.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Commands.Conditions;

namespace Christofel.CommandsLib.Permissions
{
    /// <summary>
    /// Attribute for commands that makes sure the command can be
    /// executed only if the user trying to execute it has the correct <see cref="Permission"/>.
    /// </summary>
    public class RequirePermissionAttribute : ConditionAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequirePermissionAttribute"/> class.
        /// </summary>
        /// <param name="permission">The permission string.</param>
        public RequirePermissionAttribute(string permission)
        {
            Permission = permission;
        }

        /// <summary>
        /// Gets the permission that the user has to have in order to execute the command.
        /// </summary>
        public string Permission { get; set; }
    }
}