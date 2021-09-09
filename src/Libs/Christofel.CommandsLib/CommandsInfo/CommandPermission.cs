//
//   CommandPermission.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.BaseLib.Permissions;

namespace Christofel.CommandsLib.CommandsInfo
{
    /// <summary>
    ///     Permission for a slash command
    /// </summary>
    public sealed class CommandPermission : IPermission
    {
        private readonly string _commandDescription;
        private readonly string _commandName;

        public CommandPermission(string commandName, string commandDescription, string permissionName)
        {
            PermissionName = permissionName;
            _commandName = commandName;
            _commandDescription = commandDescription;
        }

        public string PermissionName { get; }
        public string DisplayName => @$"Slash command /{_commandName}";
        public string Description => $@"This is a permission for slash command. {_commandDescription}";
    }
}