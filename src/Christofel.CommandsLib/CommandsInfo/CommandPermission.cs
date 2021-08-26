using Christofel.BaseLib.Permissions;

namespace Christofel.CommandsLib.CommandsInfo
{
    /// <summary>
    /// Permission for a slash command
    /// </summary>
    public sealed class CommandPermission : IPermission
    {
        private string _commandDescription;
        private string _commandName;

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