using System;
using System.Linq;

namespace Christofel.BaseLib.Database
{
    public interface IReadableDbContext : IDisposable, IAsyncDisposable
    {
        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class;
    }
}