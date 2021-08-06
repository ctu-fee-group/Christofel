using System.Collections.Generic;
using System.Linq;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;

namespace Christofel.BaseLib.Extensions
{
    public static class PermissionAssignmentQueryExtensions
    {
        public static IQueryable<PermissionAssignment> WhereTargetEquals(this IQueryable<PermissionAssignment> assignmentQuery, DiscordTarget target)
        {
            if (target.TargetType != TargetType.Everyone)
            {
                assignmentQuery = assignmentQuery
                    .Where(x => x.Target.DiscordId == target.DiscordId);
            }

            return assignmentQuery
                .Where(x => x.Target.TargetType == target.TargetType);
        }
        
        public static IAsyncEnumerable<PermissionAssignment> WhereTargetEquals(this IAsyncEnumerable<PermissionAssignment> assignmentQuery, DiscordTarget target)
        {
            if (target.TargetType != TargetType.Everyone)
            {
                assignmentQuery = assignmentQuery
                    .Where(x => x.Target.DiscordId == target.DiscordId);
            }

            return assignmentQuery
                .Where(x => x.Target.TargetType == target.TargetType);
        }
    }
}