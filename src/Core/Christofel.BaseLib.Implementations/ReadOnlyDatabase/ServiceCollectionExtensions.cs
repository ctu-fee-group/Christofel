using System.Data.Common;
using Christofel.BaseLib.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.BaseLib.Implementations.ReadOnlyDatabase
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddReadOnlyDbContextFactory<TContext>(this IServiceCollection services)
            where TContext : DbContext, IReadableDbContext<TContext>
        {
            return services
                .AddSingleton<ReadonlyDbContextFactory<TContext>>();
        }

        public static IServiceCollection AddReadOnlyDbContext<TContext>(this IServiceCollection services)
            where TContext : DbContext, IReadableDbContext<TContext>
        {
            return services
                .AddReadOnlyDbContextFactory<TContext>()
                .AddTransient<IReadableDbContext<TContext>>(p =>
                    p.GetRequiredService<ReadonlyDbContextFactory<TContext>>().CreateDbContext());
        }
    }
}