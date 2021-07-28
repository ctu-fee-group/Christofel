using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace Christofel.Application.Logging.Discord
{
    public static class DiscordLoggerExtensions
    {
        public static ILoggingBuilder AddDiscordLogger(
            this ILoggingBuilder builder)
        {
            builder.AddConfiguration();

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, DiscordLoggerProvider>());
            LoggerProviderOptions.RegisterProviderOptions<DiscordLoggerCofiguration, DiscordLoggerProvider>(builder.Services);

            return builder;
        }
        
        public static ILoggingBuilder AddDiscordLogger(
            this ILoggingBuilder builder,
            Action<DiscordLoggerCofiguration> configure)
        {
            builder.AddDiscordLogger();
            builder.Services.Configure(configure);

            return builder;
        }
    }
}