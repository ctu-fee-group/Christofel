//
//   UserQueryExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using Christofel.BaseLib.Database.Models;

namespace Christofel.BaseLib.Extensions
{
    public static class UserQueryExtensions
    {
        public static IQueryable<DbUser> Authenticated(this IQueryable<DbUser> userQuery)
        {
            return userQuery
                .Where(x => x.AuthenticatedAt != null);
        }
    }
}