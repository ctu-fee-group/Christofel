//
//   ManagementContext.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using Christofel.BaseLib.Database;
using Christofel.Management.Database.Models;
using Microsoft.EntityFrameworkCore;
using Remora.Discord.Core;

namespace Christofel.Management.Database
{
    public class ManagementContext : DbContext, IReadableDbContext<ManagementContext>
    {
        public ManagementContext(DbContextOptions<ManagementContext> options)
            : base(options)
        {
        }

        public DbSet<TemporalSlowmode> TemporalSlowmodes => Set<TemporalSlowmode>();

        IQueryable<TEntity> IReadableDbContext.Set<TEntity>()
            where TEntity : class => Set<TEntity>().AsNoTracking();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TemporalSlowmode>()
                .Property(x => x.ChannelId)
                .HasConversion(v => (long) v.Value, v => new Snowflake((ulong) v));

            modelBuilder.Entity<TemporalSlowmode>()
                .Property(x => x.UserId)
                .HasConversion(v => (long) v.Value, v => new Snowflake((ulong) v));
        }
    }
}