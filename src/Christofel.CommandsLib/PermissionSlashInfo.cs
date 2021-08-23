using Discord;
using Discord.Net.Interactions.Abstractions;

namespace Christofel.CommandsLib
{
    public class PermissionSlashInfo : SlashCommandInfo
    {
        public PermissionSlashInfo(SlashCommandBuilder builder, InstancedDiscordInteractionHandler instancedHandler,
            string permission, bool global) : base(builder, instancedHandler, global)
        {
            Permission = permission;
        }

        public PermissionSlashInfo(SlashCommandBuilder builder, DiscordInteractionHandler handler, string permission,
            bool global) : base(builder, handler, global)
        {
            Permission = permission;
        }

        public string Permission { get; set; }
    }
}