//
//   ApiCacheContext.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using Christofel.BaseLib.Database;
using Microsoft.EntityFrameworkCore;
using Remora.Discord.Core;
using Remora.EntityFrameworkCore.Modular;

namespace Christofel.Api.Ctu.Database
{
    /// <summary>
    /// Database context holding the roles to be assigned.
    /// </summary>
    public sealed class ApiCacheContext : SchemaAwareDbContext, IReadableDbContext
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

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AssignRole>()
                .Property(x => x.RoleId)
                .HasConversion(v => (long)v.Value, v => new Snowflake((ulong)v));

            modelBuilder.Entity<AssignRole>()
                .Property(x => x.GuildDiscordId)
                .HasConversion(v => (long)v.Value, v => new Snowflake((ulong)v));

            modelBuilder.Entity<AssignRole>()
                .Property(x => x.UserDiscordId)
                .HasConversion(v => (long)v.Value, v => new Snowflake((ulong)v));
        }
    }
}