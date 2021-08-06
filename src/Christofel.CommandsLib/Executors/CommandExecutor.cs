using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.CommandsLib.CommandsInfo;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Christofel.CommandsLib.Executors
{
    /// <summary>
    /// Basic CommandExecutor calling Handlere of the command
    /// </summary>
    public class CommandExecutor : ICommandExecutor
    {
        private readonly ILogger _logger;
        
        public CommandExecutor(ILogger logger)
        {
            _logger = logger;
        }
        
        public async Task TryExecuteCommand(SlashCommandInfo info, SocketSlashCommand command, CancellationToken token = default)
        {
            try
            {
                _logger.LogInformation($@"Handling command /{command.Data.Name} executed by {command.User.Mention} ({command.User})");
                await info.Handler(command, token);
            }
            catch (OperationCanceledException)
            {
                token.ThrowIfCancellationRequested();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $@"Command handler for command /{command.Data.Name} has thrown an exception");
            }
        }
    }
}