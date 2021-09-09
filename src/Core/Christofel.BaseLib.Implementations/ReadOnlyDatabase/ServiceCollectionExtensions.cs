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
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddReadOnlyDbContextFactory<TContext>(this IServiceCollection services)
            where TContext : DbContext, IReadableDbContext<TContext>
            => services
                .AddSingleton<ReadonlyDbContextFactory<TContext>>();

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