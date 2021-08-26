using System.Linq;
using Christofel.BaseLib.Database;
using Microsoft.EntityFrameworkCore;

namespace Christofel.Api.Ctu.Database
{
    public class ApiCacheContext : DbContext, IReadableDbContext
    {
        /// <summary>
        /// Cache of roles to assign
        /// </summary>
        public DbSet<AssignRole> AssignRoles => Set<AssignRole>();
        
        IQueryable<TEntity> IReadableDbContext.Set<TEntity>() where TEntity : class
        {
            return Set<TEntity>().AsNoTracking();
        }
    }
}