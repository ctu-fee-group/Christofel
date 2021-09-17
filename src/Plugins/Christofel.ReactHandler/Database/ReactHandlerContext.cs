//
//   ReactHandlerContext.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using Christofel.Common.Database;
using Christofel.ReactHandler.Database.Models;
using Microsoft.EntityFrameworkCore;
using Remora.Discord.Core;
using Remora.EntityFrameworkCore.Modular;

namespace Christofel.ReactHandler.Database
{
    /// <summary>
    /// Database context holding what messages should be reacted to.
    /// </summary>
    public sealed class ReactHandlerContext : SchemaAwareDbContext, IReadableDbContext<ReactHandlerContext>
    {
        /// <summary>
        /// The name of the schema that this context's entities lie in.
        /// </summary>
        public const string SchemaName = "ReactHandler";

        /// <summary>
        /// Initializes a new instance of the <see cref="ReactHandlerContext"/> class.
        /// </summary>
        /// <param name="options">The options of the context.</param>
        public ReactHandlerContext(DbContextOptions<ReactHandlerContext> options)
            : base(SchemaName, options)
        {
        }

        /// <summary>
        /// Gets set of <see cref="HandleReact"/>.
        /// </summary>
        public DbSet<HandleReact> HandleReacts => Set<HandleReact>();

        /// <inheritdoc/>
        IQueryable<TEntity> IReadableDbContext.Set<TEntity>()
            where TEntity : class => Set<TEntity>().AsNoTracking();

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HandleReact>()
                .Property(x => x.ChannelId)
                .HasConversion(v => (long)v.Value, v => new Snowflake((ulong)v));

            modelBuilder.Entity<HandleReact>()
                .Property(x => x.EntityId)
                .HasConversion(v => (long)v.Value, v => new Snowflake((ulong)v));

            modelBuilder.Entity<HandleReact>()
                .Property(x => x.MessageId)
                .HasConversion(v => (long)v.Value, v => new Snowflake((ulong)v));
        }
    }
}