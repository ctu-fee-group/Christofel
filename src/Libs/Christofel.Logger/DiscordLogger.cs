//
//   DiscordLogger.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Christofel.Logger
{
    public class DiscordLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly DiscordLoggerProcessor _queueProcessor;

        public DiscordLogger(DiscordLoggerOptions config, DiscordLoggerProcessor queueProcessor, string categoryName)
        {
            _categoryName = categoryName;
            Config = config;
            _queueProcessor = queueProcessor;
        }

        public IExternalScopeProvider? ScopeProvider { get; set; }
        public DiscordLoggerOptions Config { get; set; }

#nullable disable
        public void Log<TState>
        (
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter
        )
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var messageContent = formatter(state, exception);
            if (messageContent == null)
            {
                messageContent = state?.ToString() ?? "";
            }

            if (exception != null)
            {
                messageContent += exception.ToString();
            }

            messageContent = messageContent
                .Replace("`", "\\`");

            var header = "**" + GetLevelText(logLevel) + $@" {_categoryName}[{eventId}]** {GetScopeMessage()}";

            var message = header + "\n```" + messageContent + "```";

            foreach (var channel in Config.Channels.Where(x => logLevel >= x.MinLevel))
            {
                _queueProcessor.EnqueueMessage(new DiscordLogMessage(channel.GuildId, channel.ChannelId, message));
            }
        }
#nullable enable

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public IDisposable BeginScope<TState>(TState state) => ScopeProvider?.Push(state) ?? NullScope.Instance;

        private string GetLevelText(LogLevel level) =>
            level switch
            {
                LogLevel.Information => "ℹ️",
                LogLevel.Warning => "⚠",
                LogLevel.Error => "🆘",
                LogLevel.Critical => "💀",
                _ => level.ToString(),
            };

        private string GetScopeMessage()
        {
            List<string> builder = new List<string>();
            ScopeProvider?.ForEachScope
            (
                (scope, state) =>
                {
                    state.Add(scope?.ToString() ?? "null");
                }, builder
            );

            return string.Join(" => ", builder);
        }

        private class NullScope : IDisposable
        {
            private static NullScope? _instance;
            public static NullScope Instance => _instance ??= new NullScope();

            public void Dispose()
            {
            }
        }
    }
}