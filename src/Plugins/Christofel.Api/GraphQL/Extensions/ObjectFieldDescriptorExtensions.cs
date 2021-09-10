//
//   ObjectFieldDescriptorExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.BaseLib.Database;
using Christofel.BaseLib.Implementations.ReadOnlyDatabase;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.Api.GraphQL.Extensions
{
    /// <summary>
    /// Class containing extensions for <see cref="IObjectFieldDescriptor"/>.
    /// </summary>
    public static class ObjectFieldDescriptorExtensions
    {
        /// <summary>
        /// Use database context of given type as scoped service
        /// <see cref="IDbContextFactory{TContext}"/> must be available from services.
        /// </summary>
        /// <param name="descriptor">The descriptor to use.</param>
        /// <typeparam name="TDbContext">The database context type.</typeparam>
        /// <returns>The passed object field descriptor.</returns>
        public static IObjectFieldDescriptor UseDbContext<TDbContext>
        (
            this IObjectFieldDescriptor descriptor
        )
            where TDbContext : DbContext
        {
            return descriptor.UseScopedService
            (
                s => s.GetRequiredService<IDbContextFactory<TDbContext>>().CreateDbContext(),
                disposeAsync: (s, c) => c.DisposeAsync()
            );
        }

        /// <summary>
        /// Use read only context of given type as scoped service
        /// <see cref="ReadonlyDbContextFactory{TContext}"/> must be available from services.
        /// </summary>
        /// <param name="descriptor">The descriptor to use.</param>
        /// <typeparam name="TDbContext">The database context type.</typeparam>
        /// <returns>The passed object field descriptor.</returns>
        public static IObjectFieldDescriptor UseReadOnlyDbContext<TDbContext>
        (
            this IObjectFieldDescriptor descriptor
        )
            where TDbContext : DbContext, IReadableDbContext<TDbContext>
        {
            return descriptor.UseScopedService<IReadableDbContext>
            (
                s => s.GetRequiredService<ReadonlyDbContextFactory<TDbContext>>().CreateDbContext(),
                disposeAsync: (s, c) => c.DisposeAsync()
            );
        }
    }
}