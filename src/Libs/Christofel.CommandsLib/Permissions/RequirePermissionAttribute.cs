//
//   RequirePermissionAttribute.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Commands.Conditions;

namespace Christofel.CommandsLib.Permissions
{
    public class RequirePermissionAttribute : ConditionAttribute
    {
        public RequirePermissionAttribute(string permission)
        {
            Permission = permission;
        }

        public string Permission { get; set; }
    }
}