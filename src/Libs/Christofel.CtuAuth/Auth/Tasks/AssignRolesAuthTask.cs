//
//   AssignRolesAuthTask.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.CtuAuth.Auth.Tasks
{
    /// <summary>
    /// Task for assigning roles to the user.
    /// </summary>
    public class AssignRolesAuthTask : IAuthTask
    {
        private readonly ILogger _logger;
        private readonly CtuAuthRoleAssignService _roleAssignService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignRolesAuthTask"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="roleAssignService">The service for processing roles assignment.</param>
        public AssignRolesAuthTask
        (
            ILogger<AssignRolesAuthTask> logger,
            CtuAuthRoleAssignService roleAssignService
        )
        {
            _roleAssignService = roleAssignService;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<Result> ExecuteAsync(IAuthData data, CancellationToken ct = default)
        {
            var guildMemberRoles = data.GuildUser.Roles.ToArray();

            var assignRoles = data.Roles.AddRoles
                .Select(x => x.RoleId)
                .Except(guildMemberRoles)
                .ToArray();
            var removeRoles = data.Roles.SoftRemoveRoles
                .Select(x => x.RoleId)
                .Except(data.Roles.AddRoles.Select(x => x.RoleId))
                .Intersect(guildMemberRoles)
                .ToArray();

            if (assignRoles.Length == 0 && removeRoles.Length == 0)
            {
                _logger.LogDebug("Not going to enqueue roles to assign as the member already has correct roles");
                return Result.FromSuccess();
            }

            _logger.LogDebug
            (
                $"Going to enqueue role assignments for member <@{data.DbUser.DiscordId}>. Add roles: {string.Join(", ", data.Roles.AddRoles.Select(x => x.RoleId))}. Remove roles: {string.Join(", ", data.Roles.SoftRemoveRoles.Select(x => x.RoleId))}"
            );

            // Save to cache
            try
            {
                await _roleAssignService.SaveRoles
                (
                    data.DbUser.DiscordId,
                    data.GuildId,
                    assignRoles,
                    removeRoles,
                    ct
                );
            }
            catch (Exception e)
            {
                _logger.LogWarning
                    (e, "Could not save roles to assign/remove to database, going to enqueue them anyway");
            }

            _roleAssignService.EnqueueRoles
            (
                data.GuildUser,
                data.DbUser.DiscordId,
                data.GuildId,
                assignRoles,
                removeRoles
            );

            return Result.FromSuccess();
        }
    }
}