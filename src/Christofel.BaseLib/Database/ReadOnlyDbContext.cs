using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Christofel.BaseLib.Database
{
    public class ReadOnlyDbContext : IDisposable, IAsyncDisposable
    {
        private DbContext _dbContext;
        private bool _ownsContext;

        public ReadOnlyDbContext(DbContext dbContext, bool ownsContext = true)
        {
            _dbContext = dbContext;
            _ownsContext = ownsContext;
        }

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