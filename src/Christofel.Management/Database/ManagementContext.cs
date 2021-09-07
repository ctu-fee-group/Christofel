using System.Linq;
using Christofel.BaseLib.Database;
using Christofel.Management.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Christofel.Management.Database
{
    public class ManagementContext : DbContext, IReadableDbContext<ManagementContext>
    {
        public DbSet<TemporalSlowmode> TemporalSlowmodes => Set<TemporalSlowmode>();

        IQueryable<TEntity> IReadableDbContext.Set<TEntity>() where TEntity : class
        {
            return Set<TEntity>().AsNoTracking();
        }
    }
}