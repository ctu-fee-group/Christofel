//
//   DiscordLoggerProvider.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Logger
{
    /// <summary>
    /// Logger provider logging into Discord using Remora Discord API.
    /// </summary>
    [ProviderAlias("Discord")]
    public class DiscordLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly ConcurrentDictionary<string, DiscordLogger> _loggers;
        private readonly IDisposable _onChangeToken;
        private readonly DiscordLoggerProcessor _queueProcessor;
        private DiscordLoggerOptions _config;

        private IExternalScopeProvider? _scopeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordLoggerProvider"/> class.
        /// </summary>
        /// <param name="config">The config of the provider.</param>
        /// <param name="provider">The provider of services.</param>
        public DiscordLoggerProvider(IOptionsMonitor<DiscordLoggerOptions> config, IServiceProvider provider)
        {
            _config = config.CurrentValue;
            _loggers = new ConcurrentDictionary<string, DiscordLogger>();
            _queueProcessor = new DiscordLoggerProcessor(provider, config.CurrentValue);

            _onChangeToken = config.OnChange(HandleConfigChanged);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _loggers.Clear();
            _queueProcessor.Dispose();
            _onChangeToken.Dispose();
        }

        /// <inheritdoc />
        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd
            (
                categoryName,
                category => new DiscordLogger(_config, _queueProcessor, category) { ScopeProvider = _scopeProvider }
            );
        }

        /// <inheritdoc />
        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;

            foreach (var logger in _loggers)
            {
                logger.Value.ScopeProvider = _scopeProvider;
            }
        }

        /// <summary>
        /// Replaces config in itself, all loggers and in the processor.
        /// </summary>
        /// <param name="config">New config to be set.</param>
        private void HandleConfigChanged(DiscordLoggerOptions config)
        {
            _config = config;
            _queueProcessor.Options = _config;

            foreach (DiscordLogger logger in _loggers.Values)
            {
                logger.Config = _config;
            }
        }
    }
}