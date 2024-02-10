//
//   ApiCacheContext.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace Christofel.CtuAuth.Database
{
    /// <summary>
    /// Database context holding the roles to be assigned.
    /// </summary>
    public sealed class ApiCacheContext : ChristofelContext, IReadableDbContext
    {
        /// <summary>
        /// The name of the schema that this context's entities lie in.
        /// </summary>
        public const string SchemaName = "Api";

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiCacheContext"/> class.
        /// </summary>
        /// <param name="options">The options for the context.</param>
        public ApiCacheContext(DbContextOptions<ApiCacheContext> options)
            : base(SchemaName, options)
        {
        }

        /// <summary>
        /// Gets the assign roles set.
        /// </summary>
        public DbSet<AssignRole> AssignRoles => Set<AssignRole>();

        /// <inheritdoc/>
        IQueryable<TEntity> IReadableDbContext.Set<TEntity>()
            where TEntity : class => Set<TEntity>().AsNoTracking();
    }
}