using System.Collections.Generic;
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
        
        public static IAsyncEnumerable<DbUser> Authenticated(this IAsyncEnumerable<DbUser> userQuery)
        {
            return userQuery
                .Where(x => x.AuthenticatedAt != null);
        }
    }
}