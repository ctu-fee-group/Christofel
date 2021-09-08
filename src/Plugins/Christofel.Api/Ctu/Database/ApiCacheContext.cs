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

        IQueryable<TEntity> IReadableDbContext.Set<TEntity>() where TEntity : class
        {
            return Set<TEntity>().AsNoTracking();
        }
    }
}