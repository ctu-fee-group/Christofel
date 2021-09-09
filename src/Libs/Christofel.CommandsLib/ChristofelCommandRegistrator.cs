using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.Plugins.Lifetime;
using Christofel.Plugins.Runtime;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Discord.Core;

namespace Christofel.CommandsLib
{
    public class ChristofelCommandRegistrator : IStartable, IRefreshable, IStoppable
    {
        private readonly ChristofelSlashService _slashService;
        private readonly BotOptions _options;
        private readonly ILogger _logger;
        private readonly IApplicationLifetime _lifetime;

        public ChristofelCommandRegistrator(
            ILogger<ChristofelCommandRegistrator> logger,
            ChristofelSlashService slashService,
            IApplicationLifetime lifetime,
            IOptionsSnapshot<BotOptions> options)
        {
            _lifetime = lifetime;
            _logger = logger;
            _slashService = slashService;
            _options = options.Value;
        }

        public Task StartAsync(CancellationToken token = new CancellationToken())
        {
            return RefreshAsync(token);
        }

        public async Task RefreshAsync(CancellationToken token = new CancellationToken())
        {
            var checkSlashSupport = _slashService.SupportsSlashCommands();
            if (!checkSlashSupport.IsSuccess)
            {
                _logger.LogWarning
                (
                    "The registered commands of the bot don't support slash commands: {Reason}",
                    checkSlashSupport.Error.Message
                );
            }
            else
            {
                var updateSlash = await _slashService.UpdateSlashCommandsAsync(new Snowflake(_options.GuildId), token);
                if (!updateSlash.IsSuccess)
                {
                    _logger.LogWarning("Failed to update slash commands: {Reason}", updateSlash.Error.Message);
                }
            }
        }

        public async Task StopAsync(CancellationToken token = new CancellationToken())
        {
            if (_lifetime.State >= LifetimeState.Stopping)
            {
                // Do not unregister commands on application exit
                return;
            }
            
            var checkSlashSupport = _slashService.SupportsSlashCommands();
            if (!checkSlashSupport.IsSuccess)
            {
                _logger.LogWarning
                (
                    "The registered commands of the bot don't support slash commands: {Reason}",
                    checkSlashSupport.Error.Message
                );
            }
            else
            {
                var updateSlash =
                    await _slashService.DeleteSlashCommandsAsync(new Snowflake(_options.GuildId), token);
                if (!updateSlash.IsSuccess)
                {
                    _logger.LogWarning("Failed to delete slash commands: {Reason}", updateSlash.Error.Message);
                }
            }
        }
    }
}