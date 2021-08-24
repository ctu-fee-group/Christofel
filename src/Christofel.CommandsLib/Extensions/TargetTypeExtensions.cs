using System;
using Christofel.BaseLib.Database.Models.Enums;

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
        public static ApplicationCommandPermissionTarget AsApplicationCommandPermission(this TargetType targetType)
        {
            switch (targetType)
            {
                case TargetType.Role:
                    return ApplicationCommandPermissionTarget.Role;
                case TargetType.User:
                    return ApplicationCommandPermissionTarget.User;
            }

            throw new ArgumentException($@"Cannot cast {targetType} to ApplicationCommandPermissionTarget");
        }
    }
}