using Christofel.BaseLib.Permissions;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace Christofel.CommandsLib.Commands
{
    public sealed class CommandPermission : IPermission
    {
        private string _commandDescription;
        private string _commandName;
        
        public CommandPermission(SlashCommandBuilder builder, string permissionName)
        {
            PermissionName = permissionName;
            _commandName = builder.Name;
            _commandDescription = builder.Description;
        }
        
        public CommandPermission(SlashCommandCreationProperties commandProperties, string permissionName)
        {
            PermissionName = permissionName;
            _commandName = commandProperties.Name;
            _commandDescription = commandProperties.Description;
        }

        public CommandPermission(IApplicationCommand command, string permissionName)
        {
            PermissionName = permissionName;
            _commandName = command.Name;
            _commandDescription = command.Description;
        }

        public string PermissionName { get; }
        public string DisplayName => @$"Slash command /{_commandName}";
        public string Description => $@"This is a permission for slash command. {_commandDescription}";
    }
}