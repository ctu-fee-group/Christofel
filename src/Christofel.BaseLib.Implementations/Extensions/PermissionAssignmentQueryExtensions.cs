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

        public static IAsyncEnumerable<PermissionAssignment> WhereTargetAnyOf(this IAsyncEnumerable<PermissionAssignment> assignmentQuery,
            IEnumerable<DiscordTarget> targets)
        {
            DiscordTarget[]? discordTargets = targets as DiscordTarget[] ?? targets.ToArray();
            IEnumerable<ulong> roles = discordTargets.Where(x => x.TargetType == TargetType.Role).Select(x => x.DiscordId);
            IEnumerable<ulong> users = discordTargets.Where(x => x.TargetType == TargetType.User).Select(x => x.DiscordId);
            bool everyone = discordTargets.Any(x => x.TargetType == TargetType.Everyone);

            return assignmentQuery
                .Where(x => (everyone && x.Target.TargetType == TargetType.Everyone) ||
                            (x.Target.TargetType == TargetType.Role && roles.Contains(x.Target.DiscordId)) ||
                            (x.Target.TargetType == TargetType.User && users.Contains(x.Target.DiscordId)));
        }
        
        public static IQueryable<PermissionAssignment> WhereTargetAnyOf(this IQueryable<PermissionAssignment> assignmentQuery,
            IEnumerable<DiscordTarget> targets)
        {
            bool everyone = false;
            List<ulong> roles = new List<ulong>();
            List<ulong> users = new List<ulong>();

            foreach (DiscordTarget target in targets)
            {
                switch (target.TargetType)
                {
                    case TargetType.Everyone:
                        everyone = true;
                        break;
                    case TargetType.Role:
                        roles.Add(target.DiscordId);
                        break;
                    case TargetType.User:
                        users.Add(target.DiscordId);
                        break;
                }
            }
            
            return assignmentQuery
                .Where(x => (everyone && x.Target.TargetType == TargetType.Everyone) ||
                            (x.Target.TargetType == TargetType.Role && roles.Contains(x.Target.DiscordId)) ||
                            (x.Target.TargetType == TargetType.User && users.Contains(x.Target.DiscordId)));
        }
    }
}