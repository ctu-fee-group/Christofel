using System.Collections.Generic;
using System.Linq;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;
using Remora.Discord.Core;

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

        public static IQueryable<PermissionAssignment> WhereTargetAnyOf(this IQueryable<PermissionAssignment> assignmentQuery,
            IEnumerable<DiscordTarget> targets)
        {
            bool everyone = false;
            List<Snowflake> roles = new List<Snowflake>();
            List<Snowflake> users = new List<Snowflake>();

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