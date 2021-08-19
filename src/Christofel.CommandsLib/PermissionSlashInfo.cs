using Discord;
using Discord.Net.Interactions.Abstractions;

namespace Christofel.CommandsLib
{
    public class PermissionSlashInfo : SlashCommandInfo
    {
        public PermissionSlashInfo(SlashCommandBuilder builder, SlashCommandHandler handler, string permission, bool global,
            ulong? guildId) : base(builder, handler, global, guildId)
        {
            Permission = permission;
        }
        
        public string Permission { get; set; }
    }
}