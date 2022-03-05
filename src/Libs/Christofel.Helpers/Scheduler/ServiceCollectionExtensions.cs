//
//   ServiceCollectionExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Common;
using Christofel.Plugins;
using Christofel.Scheduling;
using Christofel.Scheduling.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Christofel.Helpers.Scheduler
{
    /// <summary>
    /// Class containing extensions for <see cref="IServiceCollection"/> that are targeted for adding scheduler to plugins.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds <see cref="IScheduler"/>.
        /// </summary>
        /// <param name="services">The collection to be configured.</param>
        /// <param name="applicationScheduler">The scheduler of the whole application passed in <see cref="IChristofelState"/>.</param>
        /// <returns>The passed collection.</returns>
        public static IServiceCollection AddPluginScheduler
            (this IServiceCollection services, IScheduler applicationScheduler) => services
            .AddSingleton<IJobExecutor, PluginExecutor>()
            .AddSingleton<IJobThreadScheduler, JobThreadScheduler>()
            .AddSingleton<SchedulerEventExecutors>()
            .AddSingleton<PluginJobsRepository>(p => p.GetRequiredService<IOptions<PluginJobsRepository>>().Value)
            .AddStateful<PluginScheduling>(ServiceLifetime.Transient)
            .AddJobListener<LoggingJobListener>()
            .AddSingleton<IScheduler>(applicationScheduler);

        /// <summary>
        /// Adds the given job to the <see cref="PluginJobsRepository"/>.
        /// </summary>
        /// <param name="services">The collection to be configured.</param>
        /// <typeparam name="TJob">The type of the job.</typeparam>
        /// <returns>The passed collection.</returns>
        public static IServiceCollection AddSchedulerJob<TJob>(this IServiceCollection services) =>
            services.Configure<PluginJobsRepository>(repository => repository.RegisterType(typeof(TJob)));
    }
}