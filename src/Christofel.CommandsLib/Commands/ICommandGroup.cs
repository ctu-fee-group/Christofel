using System.Threading;
using System.Threading.Tasks;

namespace Christofel.CommandsLib.Commands
{
    /// <summary>
    /// Group of commands exposing a function for registering the commands
    /// </summary>
    public interface ICommandGroup
    {
        /// <summary>
        /// Builds and registers slash commands
        /// </summary>
        /// <param name="holder"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task SetupCommandsAsync(ICommandHolder holder, CancellationToken token = new CancellationToken());
    }
}