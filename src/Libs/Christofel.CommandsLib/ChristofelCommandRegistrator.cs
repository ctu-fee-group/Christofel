//
//   ChristofelCommandRegistrator.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Extensions;
using Christofel.Plugins.Lifetime;
using Christofel.Plugins.Runtime;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Discord.API;
using Remora.Rest.Core;

namespace Christofel.CommandsLib
{
    /// <summary>
    /// Service for managing slash commands in <see cref="DIRuntimePlugin{TState,TContext}"/>.
    /// </summary>
    /// <remarks>
    /// Registers commands on start, refreshes permissions on refresh and deletes the commands on stop.
    ///
    /// If the whole application is closing, then the commands will not be deleted, but instead treated
    /// like they will be added again next start of the bot.
    /// </remarks>
    public class ChristofelCommandRegistrator : IStartable, IRefreshable, IStoppable
    {
        private readonly IApplicationLifetime _lifetime;
        private readonly ILogger _logger;
        private readonly BotOptions _options;
        private readonly ChristofelSlashService _slashService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChristofelCommandRegistrator"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="slashService">The service used for registering the commands.</param>
        /// <param name="lifetime">The lifetime of the application.</param>
        /// <param name="options">The options of the bot.</param>
        public ChristofelCommandRegistrator
        (
            ILogger<ChristofelCommandRegistrator> logger,
            ChristofelSlashService slashService,
            IApplicationLifetime lifetime,
            IOptionsSnapshot<BotOptions> options
        )
        {
            _lifetime = lifetime;
            _logger = logger;
            _slashService = slashService;
            _options = options.Value;
        }

        /// <inheritdoc />
        public async Task RefreshAsync(CancellationToken token = default)
        {
            var checkSlashSupport = _slashService.SupportsSlashCommands();
            if (!checkSlashSupport.IsSuccess)
            {
                _logger.LogResultError(checkSlashSupport, "The registered commands of the bot don't support slash commands");
            }
            else
            {
                var updateSlash = await _slashService.UpdateSlashCommandsAsync
                    (DiscordSnowflake.New(_options.GuildId), token);
                if (!updateSlash.IsSuccess)
                {
                    _logger.LogResultError(updateSlash, "Failed to update slash commands");
                }
            }
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken token = default) => RefreshAsync(token);

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken token = default)
        {
            if (_lifetime.State >= LifetimeState.Stopping)
            {
                // Do not unregister commands on application exit
                return;
            }

            var checkSlashSupport = _slashService.SupportsSlashCommands();
            if (checkSlashSupport.IsSuccess)
            {
                var updateSlash =
                    await _slashService.DeleteSlashCommandsAsync(DiscordSnowflake.New(_options.GuildId), token);
                if (!updateSlash.IsSuccess)
                {
                    _logger.LogResultError(updateSlash, "Failed to delete slash commands.");
                }
            }
        }
    }
}