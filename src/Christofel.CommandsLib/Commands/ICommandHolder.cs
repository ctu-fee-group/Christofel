using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Christofel.CommandsLib.CommandsInfo;
using Christofel.CommandsLib.Executors;
using Discord;

namespace Christofel.CommandsLib.Commands
{
    /// <summary>
    /// Interface supporting registration and holding of slash commands
    /// </summary>
    public interface ICommandHolder
    {
        public IEnumerable<HeldSlashCommand> Commands { get; }
        
        /// <summary>
        /// Holds SlashCommand information, so it is known what executor to execute
        /// </summary>
        /// <param name="Info"></param>
        /// <param name="Executor"></param>
        public record HeldSlashCommand(SlashCommandInfo Info, ICommandExecutor Executor);

        /// <summary>
        /// Tries to get a slash command in list of commands by its name
        /// If command is not found, null is returned
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Slash command if it was found, otherwise null</returns>
        public HeldSlashCommand? TryGetSlashCommand(string name);
        
        /// <summary>
        /// Save command to collection
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="executor"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public SlashCommandInfo AddCommand(SlashCommandInfoBuilder builder, ICommandExecutor executor);
        
        /// <summary>
        /// Remove all commands from collection
        /// </summary>
        /// <param name="token"></param>
        public void RemoveCommands();
    }
}