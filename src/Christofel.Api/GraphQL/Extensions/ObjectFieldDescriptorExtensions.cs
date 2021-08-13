using Christofel.BaseLib.Database;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.Api.GraphQL.Extensions
{
    public static class ObjectFieldDescriptorExtensions
    {
        /// <summary>
        /// Use database context of given type as scoped service
        /// IDbContextFactory<TDbContext> must be available from services
        /// </summary>
        /// <param name="descriptor"></param>
        /// <typeparam name="TDbContext"></typeparam>
        /// <returns></returns>
        public static IObjectFieldDescriptor UseDbContext<TDbContext>(
            this IObjectFieldDescriptor descriptor)
            where TDbContext : DbContext
        {
            return descriptor.UseScopedService<TDbContext>(
                create: s => s.GetRequiredService<IDbContextFactory<TDbContext>>().CreateDbContext(),
                disposeAsync: (s, c) => c.DisposeAsync());
        }
        
        /// <summary>
        /// Use read only context of given type as scoped service
        /// ReadOnlyDbContextFactory<TDbContextg> muset be available from services
        /// </summary>
        /// <param name="descriptor"></param>
        /// <typeparam name="TDbContext"></typeparam>
        /// <returns></returns>
        public static IObjectFieldDescriptor UseReadOnlyDbContext<TDbContext>(
            this IObjectFieldDescriptor descriptor)
            where TDbContext : DbContext, IReadableDbContext
        {
            return descriptor.UseScopedService<IReadableDbContext>(
                create: s => s.GetRequiredService<ReadonlyDbContextFactory<TDbContext>>().CreateDbContext(),
                disposeAsync: (s, c) => c.DisposeAsync());
        }
    }
}