//
//   ServiceCollectionExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.BaseLib.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.BaseLib.Implementations.ReadOnlyDatabase
{
    /// <summary>
    /// Class containing extensions for waiting for <see cref="IServiceCollection"/> to change into some state.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds read only context factory as a singleton service for the given context.
        /// </summary>
        /// <param name="services">The collection of the services.</param>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        /// <returns>The passed collection.</returns>
        public static IServiceCollection AddReadOnlyDbContextFactory<TContext>(this IServiceCollection services)
            where TContext : DbContext, IReadableDbContext<TContext>
            => services
                .AddSingleton<ReadonlyDbContextFactory<TContext>>();

        /// <summary>
        /// Adds read only context as a transient service for the given context.
        /// </summary>
        /// <param name="services">The collection of the services.</param>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        /// <returns>The passed collection.</returns>
        public static IServiceCollection AddReadOnlyDbContext<TContext>(this IServiceCollection services)
            where TContext : DbContext, IReadableDbContext<TContext>
        {
            return services
                .AddReadOnlyDbContextFactory<TContext>()
                .AddTransient
                (
                    p =>
                        p.GetRequiredService<ReadonlyDbContextFactory<TContext>>().CreateDbContext()
                );
        }
    }
}