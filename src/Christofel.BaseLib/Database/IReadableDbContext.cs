using System;
using System.Linq;

namespace Christofel.BaseLib.Database
{
    /// <summary>
    /// Context that supports reading
    /// </summary>
    /// <remarks>
    /// May be used in cases when it isn't known/important whether
    /// the context is ReadOnlyDbContext or regular DbContext
    /// </remarks>
    public interface IReadableDbContext : IDisposable, IAsyncDisposable
    {
        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class;
    }
}