//
//   ReadonlyDbContextFactory.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace Christofel.Helpers.ReadOnlyDatabase
{
    /// <summary>
    /// Creates ReadonlyDbContext for specified DbContextFactory.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    public class ReadonlyDbContextFactory<TContext>
        where TContext : DbContext, IReadableDbContext<TContext>
    {
        private readonly IDbContextFactory<TContext> _dbContextFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadonlyDbContextFactory{TContext}"/> class.
        /// </summary>
        /// <param name="dbContextFactory">The database context factory.</param>
        public ReadonlyDbContextFactory(IDbContextFactory<TContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        /// <summary>
        /// Creates the readable context.
        /// </summary>
        /// <returns>Read only context representing <typeparamref name="TContext"/>.</returns>
        public virtual IReadableDbContext<TContext> CreateDbContext() => _dbContextFactory.CreateDbContext();
    }
}