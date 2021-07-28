using System;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using Christofel.BaseLib.Discord;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Application.Logging.Discord
{
    public class DiscordLoggerProvider : ILoggerProvider
    {
        private readonly IDisposable _onChangeToken;
        private readonly ConcurrentDictionary<string, DiscordLogger> _loggers;
        private readonly DiscordLoggerProcessor _queueProcessor;
        private DiscordLoggerCofiguration _config;

        public DiscordLoggerProvider(IOptionsMonitor<DiscordLoggerCofiguration> config, IBot bot)
        {
            _config = config.CurrentValue;
            _loggers = new ConcurrentDictionary<string, DiscordLogger>();
            _queueProcessor = new DiscordLoggerProcessor(bot);
            
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
            return _loggers.GetOrAdd(categoryName, category => new DiscordLogger(_config, _queueProcessor, categoryName));
        }

        private void HandleConfigChanged(DiscordLoggerCofiguration config)
        {
            _config = config;

            foreach (DiscordLogger logger in _loggers.Values)
            {
                logger.Config = _config;
            }
        }
    }
}