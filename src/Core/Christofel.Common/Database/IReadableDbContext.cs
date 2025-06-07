//
//   IReadableDbContext.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

#pragma warning disable SA1402

namespace Christofel.Common.Database
{
    /// <summary>
    /// Representing database context that supports reading.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    public interface IReadableDbContext<TContext> : IReadableDbContext
        where TContext : DbContext
    {
    }

    /// <summary>
    /// Representing database context that supports reading.
    /// </summary>
    public interface IReadableDbContext : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Creates a <see cref="IQueryable{TEntity}" /> that can be used to query instances of the entity.
        /// </summary>
        /// <typeparam name="TEntity"> The type of entity for which a set should be returned. </typeparam>
        /// <returns> A set for the given entity type. </returns>
        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class;
    }
}