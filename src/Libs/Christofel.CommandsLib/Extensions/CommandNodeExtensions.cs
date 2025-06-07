//
//   CommandNodeExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Reflection;
using Christofel.CommandsLib.Permissions;
using Remora.Commands.Trees.Nodes;
using Remora.Discord.Commands.Extensions;

namespace Christofel.CommandsLib.Extensions
{
    /// <summary>
    /// Class containing extensions for <see cref="IChildNode"/>.
    /// </summary>
    public static class CommandNodeExtensions
    {
        /// <summary>
        /// Gets permission that is associated with the given node, if any.
        /// </summary>
        /// <param name="commandNode">The command node to search from.</param>
        /// <returns>Permission that is associated with the given node, if any.</returns>
        public static string? GetChristofelPermission
            (this CommandNode commandNode)
            => commandNode.FindCustomAttributeOnLocalTree<RequirePermissionAttribute>()?.Permission;

        /// <summary>
        /// Gets permission that is associated with the given node, if any.
        /// </summary>
        /// <param name="groupNode">The group node to search from.</param>
        /// <returns>Permission that is associated with the given node, if any.</returns>
        public static string? GetChristofelPermission(this GroupNode groupNode)
        {
            return groupNode.GroupTypes.Select(x => x.GetCustomAttribute<RequirePermissionAttribute>())
                .FirstOrDefault()?.Permission;
        }
    }
}