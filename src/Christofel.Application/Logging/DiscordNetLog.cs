using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Christofel.Application.Logging
{
    /// <summary>
    /// Forwards log from Discord.NET library to MEL
    /// </summary>
    public class DiscordNetLog
    {
        private readonly ILogger<DiscordNetLog> _logger;

        public DiscordNetLog(ILogger<DiscordNetLog> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Registers Log event
        /// </summary>
        /// <param name="client"></param>
        public void RegisterEvents(BaseDiscordClient client)
        {
            client.Log += ForwardLog;
        }

        /// <summary>
        /// Forwards log message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task ForwardLog(LogMessage message)
        {
            using (_logger.BeginScope(message.Source))
            {
                LogLevel logLevel = FromLogSeverity(message.Severity);

                if (message.Exception != null && logLevel < LogLevel.Error)
                {
                    logLevel = LogLevel.Error;
                }
                
                _logger.Log(logLevel, message.Exception, message.Message);
            }
            
            return Task.CompletedTask;
        }

        private LogLevel FromLogSeverity(LogSeverity severity) =>
        severity switch
        {
            LogSeverity.Verbose => LogLevel.Trace,
            LogSeverity.Debug => LogLevel.Debug,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Critical => LogLevel.Critical,
            _ => LogLevel.None
        };
    }
}