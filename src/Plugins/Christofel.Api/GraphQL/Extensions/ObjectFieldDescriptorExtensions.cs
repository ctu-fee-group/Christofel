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
    public static class ObjectFieldDescriptorExtensions
    {
        /// <summary>
        /// Use database context of given type as scoped service
        /// IDbContextFactory<TDbContext> must be available from services
        /// </summary>
        /// <param name="descriptor"></param>
        /// <typeparam name="TDbContext"></typeparam>
        /// <returns></returns>
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
        /// ReadOnlyDbContextFactory<TDbContextg> muset be available from services
        /// </summary>
        /// <param name="descriptor"></param>
        /// <typeparam name="TDbContext"></typeparam>
        /// <returns></returns>
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