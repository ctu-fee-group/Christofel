//
//   PermissionAssignmentQueryExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using Christofel.Common.Database.Models;
using Christofel.Common.Database.Models.Enums;
using Remora.Discord.Core;

namespace Christofel.BaseLib.Extensions
{
    /// <summary>
    /// Class containing extensions for <see cref="IQueryable{PermissionAssisngment}"/>.
    /// </summary>
    public static class PermissionAssignmentQueryExtensions
    {
        /// <summary>
        /// Filters permission records that match the given target.
        /// </summary>
        /// <param name="assignmentQuery">The query to be filtered.</param>
        /// <param name="target">The target to be searched for.</param>
        /// <returns>Filtered queryable.</returns>
        public static IQueryable<PermissionAssignment> WhereTargetEquals
            (this IQueryable<PermissionAssignment> assignmentQuery, DiscordTarget target)
        {
            if (target.TargetType != TargetType.Everyone)
            {
                assignmentQuery = assignmentQuery
                    .Where(x => x.Target.DiscordId == target.DiscordId);
            }

            return assignmentQuery
                .Where(x => x.Target.TargetType == target.TargetType);
        }

        /// <summary>
        /// Filters permission records that match at least one of the given target.
        /// </summary>
        /// <param name="assignmentQuery">The query to be filtered.</param>
        /// <param name="targets">The targets to be searched for.</param>
        /// <returns>Filtered queryable.</returns>
        public static IQueryable<PermissionAssignment> WhereTargetAnyOf
        (
            this IQueryable<PermissionAssignment> assignmentQuery,
            IEnumerable<DiscordTarget> targets
        )
        {
            var everyone = false;
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
                .Where
                (
                    x => (everyone && x.Target.TargetType == TargetType.Everyone) ||
                         (x.Target.TargetType == TargetType.Role && roles.Contains(x.Target.DiscordId)) ||
                         (x.Target.TargetType == TargetType.User && users.Contains(x.Target.DiscordId))
                );
        }
    }
}