using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Rest;

namespace Christofel.Application.Logging.Discord
{
    [ProviderAlias("Discord")]
    public class DiscordLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly IDisposable _onChangeToken;
        private readonly ConcurrentDictionary<string, DiscordLogger> _loggers;
        private readonly DiscordLoggerProcessor _queueProcessor;
        private DiscordLoggerOptions _config;
        
        private IExternalScopeProvider? _scopeProvider;

        public DiscordLoggerProvider(IOptionsMonitor<DiscordLoggerOptions> config, IServiceProvider provider)
        {
            _config = config.CurrentValue;
            _loggers = new ConcurrentDictionary<string, DiscordLogger>();
            _queueProcessor = new DiscordLoggerProcessor(provider, config.CurrentValue);
            
            _onChangeToken = config.OnChange(HandleConfigChanged);
        }

        public void Dispose()
        {
            _loggers.Clear();
            _queueProcessor.Dispose();
            _onChangeToken.Dispose();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, category => new DiscordLogger(_config, _queueProcessor, categoryName)
            {
                ScopeProvider = _scopeProvider
            });
        }

        private void HandleConfigChanged(DiscordLoggerOptions config)
        {
            _config = config;
            _queueProcessor.Options = _config;

            foreach (DiscordLogger logger in _loggers.Values)
            {
                logger.Config = _config;
            }
        }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;

            foreach (System.Collections.Generic.KeyValuePair<string, DiscordLogger> logger in _loggers)
            {
                logger.Value.ScopeProvider = _scopeProvider;
            }
        }
    }
}