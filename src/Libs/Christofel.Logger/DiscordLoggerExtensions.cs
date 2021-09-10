//
//   DiscordLoggerExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace Christofel.Logger
{
    /// <summary>
    /// Class containing extensions for waiting for <see cref="ILoggingBuilder"/> to change into some state.
    /// </summary>
    public static class DiscordLoggerExtensions
    {
        /// <summary>
        /// Adds <see cref="DiscordLoggerProvider"/> to log into Discord.
        /// </summary>
        /// <param name="builder">The builder of the log.</param>
        /// <returns>The passed builder.</returns>
        public static ILoggingBuilder AddDiscordLogger
        (
            this ILoggingBuilder builder
        )
        {
            builder.AddConfiguration();

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, DiscordLoggerProvider>());
            LoggerProviderOptions.RegisterProviderOptions<DiscordLoggerOptions, DiscordLoggerProvider>
                (builder.Services);

            return builder;
        }

        /// <summary>
        /// Adds <see cref="DiscordLoggerProvider"/> to log into Discord.
        /// </summary>
        /// <param name="builder">The builder of the log.</param>
        /// <param name="configure">The action for configuring options of the logger.</param>
        /// <returns>The passed builder.</returns>
        public static ILoggingBuilder AddDiscordLogger
        (
            this ILoggingBuilder builder,
            Action<DiscordLoggerOptions> configure
        )
        {
            builder.AddDiscordLogger();
            builder.Services.Configure(configure);

            return builder;
        }
    }
}