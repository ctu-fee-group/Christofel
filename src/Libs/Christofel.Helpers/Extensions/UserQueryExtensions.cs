//
//   UserQueryExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using Christofel.Common.Database.Models;

namespace Christofel.BaseLib.Extensions
{
    /// <summary>
    /// Class containing extensions for <see cref="IQueryable{DbUser}"/>.
    /// </summary>
    public static class UserQueryExtensions
    {
        /// <summary>
        /// Filters only authenticated users.
        /// </summary>
        /// <remarks>
        /// Filters users that have AuthenticatedAt set.
        /// </remarks>
        /// <param name="userQuery">The user query to be filtered.</param>
        /// <returns>Filtered queryable.</returns>
        public static IQueryable<DbUser> Authenticated(this IQueryable<DbUser> userQuery)
        {
            return userQuery
                .Where(x => x.AuthenticatedAt != null);
        }
    }
}