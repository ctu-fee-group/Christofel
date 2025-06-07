//
//   ServiceCollectionExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Plugins.Runtime;
using Christofel.Plugins.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

        /// <summary>
        /// Adds the given type as a service and tries to add it to <see cref="IStartable"/>, <see cref="IStoppable"/>, <see cref="IRefreshable"/> service types as well.
        /// </summary>
        /// <remarks>
        /// Provides the service of type <typeparamref name="TStateful"/> with the given lifetime and tries
        /// to register it as stateful types.
        ///
        /// Registers the service as IStartable, if it implements <see cref="IStartable"/>.
        /// Registers the service as IStoppable, if it implements <see cref="IStoppable"/>.
        /// Registers the service as IRefreshable, if it implements <see cref="IRefreshable"/>.
        /// </remarks>
        /// <param name="services">The services to configure.</param>
        /// <param name="lifetime">The lifetime of the service.</param>
        /// <typeparam name="TStateful">The type of the service to be added.</typeparam>
        /// <returns>The passed service collection.</returns>
        public static IServiceCollection AddStateful<TStateful>
            (this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TStateful : notnull
        {
            services.Add(ServiceDescriptor.Describe(typeof(TStateful), typeof(TStateful), lifetime));

            if (typeof(TStateful).IsAssignableTo(typeof(IStartable)))
            {
                services.Add
                    (ServiceDescriptor.Describe(typeof(IStartable), p => p.GetRequiredService<TStateful>(), lifetime));
            }

            if (typeof(TStateful).IsAssignableTo(typeof(IStoppable)))
            {
                services.Add
                    (ServiceDescriptor.Describe(typeof(IStoppable), p => p.GetRequiredService<TStateful>(), lifetime));
            }

            if (typeof(TStateful).IsAssignableTo(typeof(IRefreshable)))
            {
                services.Add
                    (ServiceDescriptor.Describe(typeof(IRefreshable), p => p.GetRequiredService<TStateful>(), lifetime));
            }

            return services;
        }
    }
}