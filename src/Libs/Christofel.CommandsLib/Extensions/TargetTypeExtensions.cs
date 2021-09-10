//
//   TargetTypeExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Christofel.BaseLib.Database.Models.Enums;
using Remora.Discord.API.Abstractions.Objects;

namespace Christofel.CommandsLib.Extensions
{
    /// <summary>
    /// Class containing extensions for <see cref="TargetType"/>.
    /// </summary>
    public static class TargetTypeExtensions
    {
        /// <summary>
        /// Obtain ApplicationCommandPermissionTarget from TargetType.
        /// </summary>
        /// <param name="targetType">The target type to convert.</param>
        /// <returns>Converted permission type.</returns>
        /// <exception cref="ArgumentException">Thrown if TargetType is nor role, nor user.</exception>
        public static ApplicationCommandPermissionType AsApplicationCommandPermission(this TargetType targetType)
        {
            switch (targetType)
            {
                case TargetType.Role:
                    return ApplicationCommandPermissionType.Role;
                case TargetType.User:
                    return ApplicationCommandPermissionType.User;
            }

            throw new ArgumentException($@"Cannot cast {targetType} to ApplicationCommandPermissionTarget");
        }
    }
}