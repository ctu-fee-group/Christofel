//
//   DbContextOptionsDisposable.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Christofel.CtuAuth.Tests.Data;

/// <summary>
/// Disposable options for dbcontext, useful for testing.
/// Adapted from EfCore.TestSupport.
/// </summary>
/// <typeparam name="T">The database context type.</typeparam>
public class DbContextOptionsDisposable<T> : DbContextOptions<T>, IDisposable
    where T : DbContext
{
    private readonly DbConnection _connection;
    private bool _preventDispose = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbContextOptionsDisposable{T}"/> class.
    /// </summary>
    /// <param name="baseOptions">The options with active connection.</param>
    public DbContextOptionsDisposable(DbContextOptions<T> baseOptions)
        : base(new ReadOnlyDictionary<Type, IDbContextOptionsExtension>(
                   baseOptions.Extensions.ToDictionary(x => x.GetType())))
    {
        _connection = RelationalOptionsExtension.Extract(baseOptions).Connection!;
    }

    /// <summary>
    /// Prevent disposal on call to Dispose.
    /// </summary>
    public void PreventDispose()
        => _preventDispose = true;

    /// <summary>
    /// Dispose even if prevented.
    /// </summary>
    public void ManualDispose()
    {
        _preventDispose = false;
        Dispose();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_preventDispose)
        {
            _connection.Dispose();
        }
    }
}
