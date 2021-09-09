//
//   IReadableDbContext.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Christofel.BaseLib.Database
{
    /// <summary>
    ///     Representing database context that supports reading
    /// </summary>
    public interface IReadableDbContext<TContext> : IReadableDbContext
        where TContext : DbContext
    {
    }


    /// <summary>
    ///     Representing database context that supports reading
    /// </summary>
    public interface IReadableDbContext : IDisposable, IAsyncDisposable
    {
        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class;
    }
}