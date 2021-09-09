//
//   IServiceCollectionExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Implementations.ReadOnlyDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Remora.Discord.Gateway.Extensions;

namespace Christofel.BaseLib.Extensions
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds Christofel state and it's properties to provider
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IServiceCollection AddDiscordState(this IServiceCollection provider, IChristofelState state)
        {
            return provider
                .AddDiscordGateway(_ => throw new InvalidOperationException("Token is obtained in the application"))
                .Replace(ServiceDescriptor.Singleton(state.Bot.Client))
                .Replace(ServiceDescriptor.Singleton(state.Bot.HttpClientFactory))
                .Replace(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)))
                .Replace(ServiceDescriptor.Singleton(state.LoggerFactory))
                .Replace(ServiceDescriptor.Singleton(state.Configuration))
                .AddSingleton(state)
                .AddSingleton(state.Permissions.Resolver)
                .AddSingleton(state.Bot)
                .AddSingleton(state.Lifetime)
                .AddSingleton(state.Permissions);
        }

        /// <summary>
        ///     Adds Christofel database context factory and read only database factory
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IServiceCollection AddChristofelDatabase
        (
            this IServiceCollection provider,
            IChristofelState state,
            bool write = true
        )
        {
            if (write)
            {
                provider
                    .AddSingleton(state.DatabaseFactory)
                    .AddTransient
                        (p => p.GetRequiredService<IDbContextFactory<ChristofelBaseContext>>().CreateDbContext());
            }

            return provider
                .AddReadOnlyDbContextFactory<ChristofelBaseContext>()
                .AddReadOnlyDbContext<ChristofelBaseContext>();
        }
    }
}