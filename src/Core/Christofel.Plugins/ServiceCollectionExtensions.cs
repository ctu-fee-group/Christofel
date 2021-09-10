//
//   ServiceCollectionExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Plugins.Runtime;
using Christofel.Plugins.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.Plugins
{
    /// <summary>
    /// Class containing extensions for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds support for plugins to the service collection using <see cref="PluginService"/>.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The passed service collection.</returns>
        public static IServiceCollection AddPlugins(this IServiceCollection services)
        {
            services
                .AddOptions<PluginServiceOptions>();

            return services
                .AddSingleton<PluginStorage>()
                .AddSingleton<PluginAssemblyService>()
                .AddSingleton<PluginLifetimeService>()
                .AddSingleton<PluginService>();
        }

        /// <summary>
        /// Adds support for specified type of runtime plugins.
        /// </summary>
        /// <remarks>
        /// Adds <see cref="RuntimePluginService{TState,TContext}"/> to the collection
        /// that will handle initialization and destroyal of <see cref="IRuntimePlugin{TState,TContext}"/>.
        /// </remarks>
        /// <param name="services">The service collection.</param>
        /// <typeparam name="TState">The state of the application that is given to the plugin.</typeparam>
        /// <typeparam name="TContext">The context of the plugin that is given to the application.</typeparam>
        /// <returns>The passed service collection.</returns>
        public static IServiceCollection AddRuntimePlugins<TState, TContext>(this IServiceCollection services)
        {
            return services
                .AddSingleton<RuntimePluginService<TState, TContext>>()
                .AddSingleton<IPluginLifetimeService, RuntimePluginService<TState, TContext>>
                (
                    p =>
                        p.GetRequiredService<RuntimePluginService<TState, TContext>>()
                );
        }
    }
}