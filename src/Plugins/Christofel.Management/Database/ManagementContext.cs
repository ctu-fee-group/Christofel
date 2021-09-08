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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TemporalSlowmode>()
                .Property(x => x.ChannelId)
                .HasConversion(v => (long)v.Value, v => new Snowflake((ulong)v));
            
            modelBuilder.Entity<TemporalSlowmode>()
                .Property(x => x.UserId)
                .HasConversion(v => (long)v.Value, v => new Snowflake((ulong)v));
        }

        IQueryable<TEntity> IReadableDbContext.Set<TEntity>() where TEntity : class
        {
            return Set<TEntity>().AsNoTracking();
        }
    }
}