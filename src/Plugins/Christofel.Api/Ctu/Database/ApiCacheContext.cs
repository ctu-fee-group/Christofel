//
//   ApiCacheContext.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using Christofel.BaseLib.Database;
using Microsoft.EntityFrameworkCore;
using Remora.Discord.Core;

namespace Christofel.Api.Ctu.Database
{
    public sealed class ApiCacheContext : DbContext, IReadableDbContext
    {
        public ApiCacheContext(DbContextOptions<ApiCacheContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Cache of roles to assign
        /// </summary>
        public DbSet<AssignRole> AssignRoles => Set<AssignRole>();

        IQueryable<TEntity> IReadableDbContext.Set<TEntity>()
            where TEntity : class => Set<TEntity>().AsNoTracking();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AssignRole>()
                .Property(x => x.RoleId)
                .HasConversion(v => (long) v.Value, v => new Snowflake((ulong) v));

            modelBuilder.Entity<AssignRole>()
                .Property(x => x.GuildDiscordId)
                .HasConversion(v => (long) v.Value, v => new Snowflake((ulong) v));

            modelBuilder.Entity<AssignRole>()
                .Property(x => x.UserDiscordId)
                .HasConversion(v => (long) v.Value, v => new Snowflake((ulong) v));
        }
    }
}