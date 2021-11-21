//
//   IServiceCollectionExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Text.Json;
using Christofel.Common;
using Christofel.Common.Database;
using Christofel.Helpers.ReadOnlyDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Gateway.Services;
using Remora.Discord.Rest;
using Remora.Discord.Rest.API;
using Remora.Discord.Rest.Extensions;
using Remora.EntityFrameworkCore.Modular;
using Remora.EntityFrameworkCore.Modular.Services;
using Remora.Rest;

namespace Christofel.BaseLib.Extensions
{
    /// <summary>
    /// Class containing extensions for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Christofel state and it's properties to provider.
        /// </summary>
        /// <param name="serviceCollection">The service collection to add state to.</param>
        /// <param name="state">The state of the Christofel application.</param>
        /// <returns>The passed collection.</returns>
        public static IServiceCollection AddDiscordState
            (this IServiceCollection serviceCollection, IChristofelState state)
        {
            serviceCollection.Replace(ServiceDescriptor.Transient(s =>
            {
                var options = s.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>().Get("Discord");
                var client = state.Bot.HttpClientFactory.CreateClient("RestHttpClient<RestError>");

                return new RestHttpClient<RestError>(client, options);
            }));
            serviceCollection.TryAddTransient<IRestHttpClient>(s => s.GetRequiredService<RestHttpClient<RestError>>());
            serviceCollection.TryAddTransient<IDiscordRestAuditLogAPI>
            (
                s => new DiscordRestAuditLogAPI
                (
                    s.GetRequiredService<IRestHttpClient>(),
                    s.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>().Get("Discord")
                )
            );

            serviceCollection.TryAddTransient<IDiscordRestChannelAPI>
            (
                s => new DiscordRestChannelAPI
                (
                    s.GetRequiredService<IRestHttpClient>(),
                    s.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>().Get("Discord")
                )
            );

            serviceCollection.TryAddTransient<IDiscordRestEmojiAPI>
            (
                s => new DiscordRestEmojiAPI
                (
                    s.GetRequiredService<IRestHttpClient>(),
                    s.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>().Get("Discord")
                )
            );

            serviceCollection.TryAddTransient<IDiscordRestGatewayAPI>
            (
                s => new DiscordRestGatewayAPI
                (
                    s.GetRequiredService<IRestHttpClient>(),
                    s.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>().Get("Discord")
                )
            );

            serviceCollection.TryAddTransient<IDiscordRestGuildAPI>
            (
                s => new DiscordRestGuildAPI
                (
                    s.GetRequiredService<IRestHttpClient>(),
                    s.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>().Get("Discord")
                )
            );

            serviceCollection.TryAddTransient<IDiscordRestGuildScheduledEventAPI>
            (
                s => new DiscordRestGuildScheduledEventAPI
                (
                    s.GetRequiredService<IRestHttpClient>(),
                    s.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>().Get("Discord")
                )
            );

            serviceCollection.TryAddTransient<IDiscordRestInviteAPI>
            (
                s => new DiscordRestInviteAPI
                (
                    s.GetRequiredService<IRestHttpClient>(),
                    s.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>().Get("Discord")
                )
            );

            serviceCollection.TryAddTransient<IDiscordRestUserAPI>
            (
                s => new DiscordRestUserAPI
                (
                    s.GetRequiredService<IRestHttpClient>(),
                    s.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>().Get("Discord")
                )
            );

            serviceCollection.TryAddTransient<IDiscordRestVoiceAPI>
            (
                s => new DiscordRestVoiceAPI
                (
                    s.GetRequiredService<IRestHttpClient>(),
                    s.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>().Get("Discord")
                )
            );

            serviceCollection.TryAddTransient<IDiscordRestWebhookAPI>
            (
                s => new DiscordRestWebhookAPI
                (
                    s.GetRequiredService<IRestHttpClient>(),
                    s.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>().Get("Discord")
                )
            );

            serviceCollection.TryAddTransient<IDiscordRestTemplateAPI>
            (
                s => new DiscordRestTemplateAPI
                (
                    s.GetRequiredService<IRestHttpClient>(),
                    s.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>().Get("Discord")
                )
            );

            serviceCollection.TryAddTransient<IDiscordRestInteractionAPI>
            (
                s => new DiscordRestInteractionAPI
                (
                    s.GetRequiredService<IRestHttpClient>(),
                    s.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>().Get("Discord")
                )
            );

            serviceCollection.TryAddTransient<IDiscordRestApplicationAPI>
            (
                s => new DiscordRestApplicationAPI
                (
                    s.GetRequiredService<IRestHttpClient>(),
                    s.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>().Get("Discord")
                )
            );

            serviceCollection.TryAddTransient<IDiscordRestOAuth2API>
            (
                s => new DiscordRestOAuth2API
                (
                    s.GetRequiredService<IRestHttpClient>(),
                    s.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>().Get("Discord")
                )
            );

            serviceCollection.TryAddTransient<IDiscordRestStageInstanceAPI>
            (
                s => new DiscordRestStageInstanceAPI
                (
                    s.GetRequiredService<IRestHttpClient>(),
                    s.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>().Get("Discord")
                )
            );

            serviceCollection.TryAddTransient<IDiscordRestStickerAPI>
            (
                s => new DiscordRestStickerAPI
                (
                    s.GetRequiredService<IRestHttpClient>(),
                    s.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>().Get("Discord")
                )
            );

            serviceCollection.TryAddSingleton<IResponderTypeRepository>
                (s => s.GetRequiredService<IOptions<ResponderService>>().Value);

            return serviceCollection
                .AddSingleton(state.Bot.Client)
                .AddSingleton(state.DiscordJsonOptions)
                .AddSingleton(state.Bot.HttpClientFactory)
                .AddSingleton(typeof(ILogger<>), typeof(Logger<>))
                .AddSingleton(state.LoggerFactory)
                .AddSingleton(state.Configuration)
                .AddSingleton(state)
                .AddSingleton(state.Permissions.Resolver)
                .AddSingleton(state.Bot)
                .AddSingleton(state.Lifetime)
                .AddSingleton(state.Permissions);
        }

        /// <summary>
        /// Adds Christofel database context factory and read only database factory.
        /// </summary>
        /// <param name="provider">The service collection to add state to.</param>
        /// <param name="state">The state of the Christofel application.</param>
        /// <param name="write">Whether write should be enabled. If false, only ReadOnly context will be added.</param>
        /// <returns>The passed collection.</returns>
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
                    .AddScoped(p => p.GetRequiredService<IDbContextFactory<ChristofelBaseContext>>().CreateDbContext());
            }

            return provider
                .AddReadOnlyDbContextFactory<ChristofelBaseContext>()
                .AddReadOnlyDbContext<ChristofelBaseContext>();
        }

        /// <summary>
        /// Adds context factory that configures the given context to be added to the context awareness.
        /// </summary>
        /// <param name="serviceCollection">The collection to be configured.</param>
        /// <param name="optionsAction">The options to be called on creation of database context.</param>
        /// <typeparam name="TContext">The type of the context to be added.</typeparam>
        /// <returns>The passed service collection.</returns>
        public static IServiceCollection AddSchemaAwareDbContextFactory<TContext>
        (
            this IServiceCollection serviceCollection,
            Action<IServiceProvider, DbContextOptionsBuilder>? optionsAction = null
        )
            where TContext : SchemaAwareDbContext
        {
            serviceCollection.TryAddSingleton<SchemaAwareDbContextService>();
            return serviceCollection.AddDbContextFactory<TContext>
            (
                (provider, optionsBuilder) =>
                {
                    var contextService = provider.GetRequiredService<SchemaAwareDbContextService>();
                    contextService.ConfigureSchemaAwareContext(optionsBuilder);
                    optionsAction?.Invoke(provider, optionsBuilder);
                }
            );
        }

        /// <summary>
        /// Adds context factory that configures the given context to be added to the context awareness.
        /// Configures the context to use MySQL with the default connection string.
        /// </summary>
        /// <param name="serviceCollection">The collection to be configured.</param>
        /// <param name="configuration">The configuration to get connection string from.</param>
        /// <param name="optionsAction">The options to be called on creation of database context.</param>
        /// <typeparam name="TContext">The type of the context to be added.</typeparam>
        /// <returns>The passed service collection.</returns>
        public static IServiceCollection AddChristofelDbContextFactory<TContext>
        (
            this IServiceCollection serviceCollection,
            IConfiguration configuration,
            Action<IServiceProvider, DbContextOptionsBuilder>? optionsAction = null
        )
            where TContext : SchemaAwareDbContext
        {
            return serviceCollection
                .AddSchemaAwareDbContextFactory<TContext>
                (
                    (provider, optionsBuilder) =>
                    {
                        optionsBuilder.UseMySql
                        (
                            configuration.GetConnectionString("ChristofelBase"),
                            ServerVersion.AutoDetect(configuration.GetConnectionString("ChristofelBase")),
                            (mysqlOptions) => mysqlOptions.SchemaBehavior
                                (MySqlSchemaBehavior.Translate, (name, objectName) => $"{name}_{objectName}")
                        );

                        optionsAction?.Invoke(provider, optionsBuilder);
                    }
                );
        }
    }
}