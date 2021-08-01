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
        /// <summary>
        /// Holds SlashCommand information, so it is known what executor to execute
        /// </summary>
        /// <param name="Info"></param>
        /// <param name="Executor"></param>
        public record HeldSlashCommand(SlashCommandInfo Info, ICommandExecutor Executor);

        /// <summary>
        /// Modifies commands DefaultPermission and other permissions
        /// according to changes in database
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task RefreshCommandsAndPermissionsAsync(CancellationToken token = default);
        
        /// <summary>
        /// Tries to get a slash command in list of commands by its name
        /// If command is not found, null is returned
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Slash command if it was found, otherwise null</returns>
        public HeldSlashCommand? TryGetSlashCommand(string name);
        
        /// <summary>
        /// Register command and save it to commands collection
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="executor"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<SlashCommandInfo> RegisterCommandAsync(SlashCommandBuilder builder, ICommandExecutor executor, CancellationToken token = default);
        
        /// <summary>
        /// Unregister all commands that are stored in commands collection
        /// </summary>
        /// <param name="token"></param>
        public Task UnregisterCommandsAsync(CancellationToken token = new CancellationToken());
    }
}