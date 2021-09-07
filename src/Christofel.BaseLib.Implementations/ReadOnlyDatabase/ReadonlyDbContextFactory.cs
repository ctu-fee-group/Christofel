using Christofel.BaseLib.Database;
using Microsoft.EntityFrameworkCore;

namespace Christofel.BaseLib.Implementations.ReadOnlyDatabase
{
    /// <summary>
    /// Creates ReadonlyDbContext for specified DbContextFactory
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class ReadonlyDbContextFactory<TContext>
        where TContext : DbContext, IReadableDbContext<TContext>
    {
        protected readonly IDbContextFactory<TContext> _dbContextFactory;
        
        public ReadonlyDbContextFactory(IDbContextFactory<TContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public virtual IReadableDbContext<TContext> CreateDbContext()
        {
            return _dbContextFactory.CreateDbContext();
        }
    }
}