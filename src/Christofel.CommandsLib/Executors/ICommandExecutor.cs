using System.Threading;
using System.Threading.Tasks;
using Christofel.CommandsLib.CommandsInfo;
using Discord.WebSocket;

namespace Christofel.CommandsLib.Executors
{
    /// <summary>
    /// Executor of a slash command interaction
    /// </summary>
    public interface ICommandExecutor
    {
        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="info">Information about the slash command</param>
        /// <param name="command">Slash command interaction</param>
        /// <param name="token">Cancel token</param>
        /// <returns></returns>
        public Task TryExecuteCommand(SlashCommandInfo info, SocketSlashCommand command, CancellationToken token = default);
    }
}