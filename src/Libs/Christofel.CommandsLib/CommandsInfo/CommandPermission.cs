//
//   CommandPermission.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.BaseLib.Permissions;

namespace Christofel.CommandsLib.CommandsInfo
{
    /// <summary>
    /// Permission for a slash command.
    /// </summary>
    public sealed class CommandPermission : IPermission
    {
        private readonly string _commandDescription;
        private readonly string _commandName;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandPermission"/> class.
        /// </summary>
        /// <param name="commandName">The name of the command.</param>
        /// <param name="commandDescription">The description of the command.</param>
        /// <param name="permissionName">The name of the permission that is needed to execute the command.</param>
        public CommandPermission(string commandName, string commandDescription, string permissionName)
        {
            PermissionName = permissionName;
            _commandName = commandName;
            _commandDescription = commandDescription;
        }

        /// <inheritdoc />
        public string PermissionName { get; }

        /// <inheritdoc />
        public string DisplayName => @$"Slash command /{_commandName}";

        /// <inheritdoc />
        public string Description => $@"This is a permission for slash command. {_commandDescription}";
    }
}