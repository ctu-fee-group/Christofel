using Discord;

namespace Christofel.CommandsLib.Commands
{
    /// <summary>
    /// Builder of SlashCommandCreationProperties along with SlashCommandInfo
    /// </summary>
    public class SlashCommandBuilderInfo : SlashCommandBuilder
    {
        /// <summary>
        /// If the command should be registered as global
        /// </summary>
        public bool Global { get; set; }
        
        /// <summary>
        /// Permission name used for non-global commands
        /// </summary>
        public string? Permission { get; internal set; }
        
        /// <summary>
        /// Where the command should be added to
        /// </summary>
        public ulong? GuildId { get; internal set; }
        
        /// <summary>
        /// Handler that will be called when the command was executed by a user
        /// </summary>
        public SlashCommandHandler? Handler { get; internal set; }
    }
}