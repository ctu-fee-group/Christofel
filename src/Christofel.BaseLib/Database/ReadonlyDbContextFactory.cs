using System;
using Microsoft.EntityFrameworkCore;

namespace Christofel.BaseLib.Database
{
    /// <summary>
    /// Creates ReadonlyDbContext for specified DbContextFactory
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class ReadonlyDbContextFactory<TContext>
        where TContext : DbContext, IReadableDbContext
    {
        protected readonly IDbContextFactory<TContext> _dbContextFactory;
        
        public ReadonlyDbContextFactory(IDbContextFactory<TContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public virtual ReadOnlyDbContext CreateDbContext()
        {
            return new ReadOnlyDbContext(_dbContextFactory.CreateDbContext(), true);
        }
    }
}