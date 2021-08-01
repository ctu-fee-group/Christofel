using System.Threading;
using System.Threading.Tasks;
using Christofel.CommandsLib.CommandsInfo;
using Discord.WebSocket;

namespace Christofel.CommandsLib.Executors
{
    /// <summary>
    /// Executor that will auto defer the command.
    /// If message is supplied, respond with that message will be sent.
    /// If message is null, Defer will be used.
    /// </summary>
    public class AutoDeferCommandExecutor : ICommandExecutor
    {
        private readonly ICommandExecutor _executor;
        
        public AutoDeferCommandExecutor(ICommandExecutor underlyingExecutor, string? message = "I am thinking...")
        {
            _executor = underlyingExecutor;
            Message = message;
        }
        
        /// <summary>
        /// Message to send. If null, Defer
        /// </summary>
        public string? Message { get; set; }
        
        public async Task TryExecuteCommand(SlashCommandInfo info, SocketSlashCommand command, CancellationToken token = default)
        {
            if (Message is null)
            {
                await command
                    .DeferAsync();
            }
            else
            {
                await command
                    .RespondAsync(Message);
            }
            
            await _executor.TryExecuteCommand(info, command, token);
        }
    }
}