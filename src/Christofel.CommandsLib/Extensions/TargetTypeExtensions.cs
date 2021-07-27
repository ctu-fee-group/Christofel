using System;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;
using Discord;

namespace Christofel.CommandsLib.Extensions
{
    public static class TargetTypeExtensions
    {
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