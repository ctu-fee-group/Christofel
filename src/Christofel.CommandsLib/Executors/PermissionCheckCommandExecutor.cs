using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Permissions;
using Christofel.CommandsLib.CommandsInfo;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Christofel.CommandsLib.Executors
{
    /// <summary>
    /// Command executor decorator checking permissions
    /// </summary>
    public class PermissionCheckCommandExecutor : ICommandExecutor
    {
        private readonly ICommandExecutor _executor;
        private readonly ILogger _logger;
        private readonly IPermissionsResolver _resolver;

        public PermissionCheckCommandExecutor(ICommandExecutor underlyingExecutor, ILogger logger,
            IPermissionsResolver resolver)
        {
            _logger = logger;
            _executor = underlyingExecutor;
            _resolver = resolver;
        }

        public async Task TryExecuteCommand(SlashCommandInfo info, SocketSlashCommand command,
            CancellationToken token = default)
        {
            if (await info.HasPermissionAsync(command.User, _resolver, token))
            {
                await _executor.TryExecuteCommand(info, command, token);
            }
            else
            {
                _logger.LogWarning(
                    $@"User {command.User.Mention} ({command.User}) tried to use command /{command.Data.Name}, but doesn't have permissions to do so");
            }
        }
    }
}