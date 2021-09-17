//
//   ManagementContext.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using Christofel.Common.Database;
using Christofel.Management.Database.Models;
using Microsoft.EntityFrameworkCore;
using Remora.Discord.Core;
using Remora.EntityFrameworkCore.Modular;

namespace Christofel.Management.Database
{
    /// <summary>
    /// Management database context that holds information about slowmodes.
    /// </summary>
    public class ManagementContext : SchemaAwareDbContext, IReadableDbContext<ManagementContext>
    {
        /// <summary>
        /// The name of the schema that this context's entities lie in.
        /// </summary>
        public const string SchemaName = "Management";

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagementContext"/> class.
        /// </summary>
        /// <param name="options">The options for the context.</param>
        public ManagementContext(DbContextOptions<ManagementContext> options)
            : base(SchemaName, options)
        {
        }

        /// <summary>
        /// Gets set holding temporal slowmodes.
        /// </summary>
        public DbSet<TemporalSlowmode> TemporalSlowmodes => Set<TemporalSlowmode>();

        /// <inheritdoc/>
        IQueryable<TEntity> IReadableDbContext.Set<TEntity>()
            where TEntity : class => Set<TEntity>().AsNoTracking();

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TemporalSlowmode>()
                .Property(x => x.ChannelId)
                .HasConversion(v => (long)v.Value, v => new Snowflake((ulong)v));

            modelBuilder.Entity<TemporalSlowmode>()
                .Property(x => x.UserId)
                .HasConversion(v => (long)v.Value, v => new Snowflake((ulong)v));
        }
    }
}