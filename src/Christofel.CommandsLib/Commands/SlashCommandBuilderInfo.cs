using Discord;

namespace Christofel.CommandsLib.Commands
{
    public class SlashCommandBuilderInfo : SlashCommandBuilder
    {
        public bool Global { get; set; }
        public string? Permission { get; internal set; }
        
        public ulong? GuildId { get; internal set; }
        
        public SlashCommandHandler? Handler { get; internal set; }
    }
}