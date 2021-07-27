using Christofel.BaseLib.Permissions;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace Christofel.CommandsLib
{
    public sealed class CommandPermission : IPermission
    {
        private string _commandDescription;
        private string _commandName;
        
        public CommandPermission(SlashCommandBuilder builder, string permissionName)
        {
            _commandName = permissionName;
            _commandDescription = builder.Description;
        }
        
        public CommandPermission(SlashCommandCreationProperties commandProperties, string permissionName)
        {
            _commandName = permissionName;
            _commandDescription = commandProperties.Description;
        }

        public CommandPermission(IApplicationCommand command, string permissionName)
        {
            _commandName = permissionName;
            _commandDescription = command.Description;
        }

        public string Name => @$"Slash command /{_commandName}";
        public string Description => $@"This is a permission for slash command. {_commandDescription}";
    }
}