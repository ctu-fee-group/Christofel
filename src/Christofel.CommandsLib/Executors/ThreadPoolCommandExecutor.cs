using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.CommandsLib.CommandsInfo;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Christofel.CommandsLib.Executors
{
    /// <summary>
    /// Command executor decorator for executing the command in separate thread
    /// </summary>
    public class ThreadPoolCommandExecutor : ICommandExecutor
    {
        private readonly ILogger _logger;
        private readonly ICommandExecutor _executor;

        public ThreadPoolCommandExecutor(ILogger logger, ICommandExecutor underlyingExecutor)
        {
            _executor = underlyingExecutor;
            _logger = logger;
        }
        
        public Task TryExecuteCommand(SlashCommandInfo info, SocketSlashCommand command, CancellationToken token = default)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _executor.TryExecuteCommand(info, command);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception e)
                {
                    _logger.LogError(0, e, "Command handler has thrown an exception while running in thread");
                }
            });
            
            return Task.CompletedTask;
        }
    }
}