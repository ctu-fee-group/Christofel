//
//   SqliteInMemory.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TestSupport.EfHelpers;

namespace Christofel.CtuAuth.Tests.Data;

/// <summary>
/// A class for creating shared db context options.
/// </summary>
public class SqliteInMemory
{
    private static readonly Random _rng = new Random();

    private static string randomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
                            .Select(s => s[_rng.Next(s.Length)]).ToArray());
    }

    /// <summary>
    /// Creates database options for an sqlite in-memory database.
    /// This database will be shared between contexts, but private from
    /// other databases as the given filename is randomly generated.
    /// </summary>
    /// <typeparam name="T">The context.</typeparam>
    /// <returns>A disposable db context options connecting to a shared, but private memory database.</returns>
    public static DbContextOptionsDisposable<T> CreateOptions<T>()
        where T : DbContext
    {
        var connection = new SqliteConnection("Filename=file:" + randomString(20) + "?mode=memory&cache=shared");
        connection.Open();

        var options = new DbContextOptionsBuilder<T>()
            .UseSqlite(connection).Options;

        return new(options);
    }
}
