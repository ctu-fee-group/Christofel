//
//   ReactHandlerContext.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using Christofel.BaseLib.Database;
using Christofel.ReactHandler.Database.Models;
using Microsoft.EntityFrameworkCore;
using Remora.Discord.Core;

namespace Christofel.ReactHandler.Database
{
    public sealed class ReactHandlerContext : DbContext, IReadableDbContext<ReactHandlerContext>
    {
        public ReactHandlerContext(DbContextOptions<ReactHandlerContext> options)
            : base(options)
        {
        }

        public DbSet<HandleReact> HandleReacts => Set<HandleReact>();

        IQueryable<TEntity> IReadableDbContext.Set<TEntity>()
            where TEntity : class => Set<TEntity>().AsNoTracking();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HandleReact>()
                .Property(x => x.ChannelId)
                .HasConversion(v => (long) v.Value, v => new Snowflake((ulong) v));

            modelBuilder.Entity<HandleReact>()
                .Property(x => x.EntityId)
                .HasConversion(v => (long) v.Value, v => new Snowflake((ulong) v));

            modelBuilder.Entity<HandleReact>()
                .Property(x => x.MessageId)
                .HasConversion(v => (long) v.Value, v => new Snowflake((ulong) v));
        }
    }
}