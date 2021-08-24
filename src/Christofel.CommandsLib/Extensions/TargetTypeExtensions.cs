using System;
using Christofel.BaseLib.Database.Models.Enums;
using Remora.Discord.API.Abstractions.Objects;

namespace Christofel.CommandsLib.Extensions
{
    public static class TargetTypeExtensions
    {
        /// <summary>
        /// Cast TargetType to ApplicationCommandPermissionTarget
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
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