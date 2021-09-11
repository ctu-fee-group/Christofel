//
//   ServiceCollectionExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Scheduler.Abstractions;
using Christofel.Scheduler.Retryable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Christofel.Scheduler.Extensions
{
    /// <summary>
    /// Defines extension methods for the type <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds default <see cref="IScheduler"/> and its dependencies into the collection.
        /// </summary>
        /// <param name="services">The collection to configure.</param>
        /// <returns>The passed collection.</returns>
        public static IServiceCollection AddScheduler(this IServiceCollection services) => services
            .AddSingleton<IScheduler, Scheduler>()
            .AddSingleton<IJobThreadScheduler, JobThreadScheduler>()
            .AddSingleton<IJobStore, ImmutableListJobStore>();

        /// <summary>
        /// Adds <see cref="RetryJobListener"/> to the collection.
        /// </summary>
        /// <param name="services">The collection to configure.</param>
        /// <returns>The passed collection.</returns>
        public static IServiceCollection AddRetryableJobListener
            (this IServiceCollection services) => services.AddJobListener<RetryJobListener>();

        /// <summary>
        /// Adds <see cref="IJobListener"/> of the specified type to the collection.
        /// </summary>
        /// <param name="services">The collection to configure.</param>
        /// <typeparam name="TJobListener">The type of the job listener to add.</typeparam>
        /// <returns>The passed collection.</returns>
        public static IServiceCollection AddJobListener<TJobListener>(this IServiceCollection services)
            where TJobListener : class, IJobListener
        {
            services.TryAddSingleton<TJobListener>();

            return services
                .AddSingleton<IJobListener, TJobListener>(p => p.GetRequiredService<TJobListener>());
        }
    }
}