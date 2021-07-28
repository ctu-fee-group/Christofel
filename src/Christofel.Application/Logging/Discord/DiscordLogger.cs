using System;
using System.Collections.Generic;
using System.Text;
using Christofel.BaseLib.Discord;
using Discord;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;

namespace Christofel.Application.Logging.Discord
{
    public class DiscordLogger : ILogger
    {
        private string _categoryName;
        private IExternalScopeProvider _scopeProvider;
        private readonly DiscordLoggerProcessor _queueProcessor;

        public DiscordLogger(DiscordLoggerCofiguration config, DiscordLoggerProcessor queueProcessor, string categoryName)
        {
            _categoryName = categoryName;
            Config = config;
            _scopeProvider = new LoggerExternalScopeProvider();
            _queueProcessor = queueProcessor;
        }
        
        public DiscordLoggerCofiguration Config { get; set; }


        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }
            
            string messageContent = formatter(state, exception);
            string header = "**" + GetLevelText(logLevel) + $@"** {_categoryName}[{eventId}] {GetScopeMessage()}";

            string message = header + "\n" + messageContent;
            
            _queueProcessor.EnqueueMessage(new DiscordLogMessage(Config.GuildId, Config.ChannelId, message));
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= Config.MinLevel;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return _scopeProvider.Push(state);
        }

        private string GetLevelText(LogLevel level) =>
            level switch
            {
                LogLevel.Information => "ℹ️",
                LogLevel.Warning => "⚠",
                LogLevel.Error => "☢",
                LogLevel.Critical => "💀",
                _ => level.ToString()
            };

        private string GetScopeMessage()
        {
            List<string> builder = new List<string>();
            _scopeProvider.ForEachScope((scope, state) =>
            {
                state.Add(scope?.ToString() ?? "null");
            }, builder);

            return string.Join(" / ", builder);
        }
    }
}