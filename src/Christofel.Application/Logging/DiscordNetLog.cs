using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Christofel.Application.Logging
{
    public class DiscordNetLog
    {
        private readonly ILogger<DiscordNetLog> _logger;

        public DiscordNetLog(ILogger<DiscordNetLog> logger)
        {
            _logger = logger;
        }

        public void RegisterEvents(BaseDiscordClient client)
        {
            client.Log += ForwardLog;
        }

        public Task ForwardLog(LogMessage message)
        {
            using (_logger.BeginScope(message.Source))
            {
                _logger.Log(FromLogSeverity(message.Severity), message.Exception, message.Message);
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