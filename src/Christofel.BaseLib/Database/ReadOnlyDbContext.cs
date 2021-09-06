using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Christofel.BaseLib.Database
{
    /// <summary>
    /// Context-like class that allows only reading of DbSets.
    /// </summary>
    /// <remarks>
    /// Should be used everywhere where only reading from db is needed.
    /// </remarks>
    public class ReadOnlyDbContext<TContext> : ReadOnlyDbContext
        where TContext : DbContext
    {
        public ReadOnlyDbContext(IReadableDbContext dbContext, bool ownsContext = true) : base(dbContext, ownsContext)
        {
        }
    }
    
    /// <summary>
    /// Context-like class that allows only reading of DbSets.
    /// </summary>
    /// <remarks>
    /// Should be used everywhere where only reading from db is needed.
    /// </remarks>
    public class ReadOnlyDbContext : IDisposable, IAsyncDisposable, IReadableDbContext
    {
        private IReadableDbContext _dbContext;
        private bool _ownsContext;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext">Underlying context that may be writable or not</param>
        /// <param name="ownsContext">If true, disposes the underlying context on dispose of this object</param>
        public ReadOnlyDbContext(IReadableDbContext dbContext, bool ownsContext = true)
        {
            _dbContext = dbContext;
            _ownsContext = ownsContext;
        }

        /// <summary>
        /// Get set from underlying database
        /// </summary>
        /// <typeparam name="TEntity">Type of the entity specifies table</typeparam>
        /// <returns>Queryable readonly set</returns>
        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
        {
            return _dbContext.Set<TEntity>().AsNoTracking();
        }

        public void Dispose()
        {
            if (_ownsContext)
            {
                _dbContext.Dispose();
            }
        }

        public ValueTask DisposeAsync()
        {
            if (_ownsContext)
            {
                return _dbContext.DisposeAsync();
            }

            return ValueTask.CompletedTask;
        }
    }
}