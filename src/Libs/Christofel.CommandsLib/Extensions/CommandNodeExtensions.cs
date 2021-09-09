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
    public static class CommandNodeExtensions
    {
        public static string? GetChristofelPermission
            (this CommandNode commandNode)
            => commandNode.FindCustomAttributeOnLocalTree<RequirePermissionAttribute>()?.Permission;

        public static string? GetChristofelPermission(this GroupNode groupNode)
        {
            return groupNode.GroupTypes.Select(x => x.GetCustomAttribute<RequirePermissionAttribute>())
                .FirstOrDefault()?.Permission;
        }
    }
}