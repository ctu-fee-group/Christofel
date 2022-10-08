//
//   QueryableExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using LinqKit;

namespace Christofel.CoursesLib.Extensions;

/// <summary>
/// Extensions for <see cref="IQueryable{T}"/>.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Where any predicates are true.
    /// </summary>
    /// <param name="q">The queryable.</param>
    /// <param name="predicates">The predicates.</param>
    /// <typeparam name="T">The type.</typeparam>
    /// <returns>The same queryable.</returns>
    public static IQueryable<T> WhereAny<T>(this IQueryable<T> q, params Expression<Func<T, bool>>[] predicates)
    {
        var orPredicate = PredicateBuilder.New<T>();
        foreach (var predicate in predicates)
        {
            orPredicate = orPredicate.Or(predicate);
        }
        return q.Where(orPredicate);
    }
}