//
//   AssignRolesAuthTask.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Tasks
{
    public class AssignRolesAuthTask : IAuthTask
    {
        private readonly ILogger _logger;
        private readonly CtuAuthRoleAssignService _roleAssignService;

        public AssignRolesAuthTask
        (
            ILogger<CtuAuthProcess> logger,
            CtuAuthRoleAssignService roleAssignService
        )
        {
            _roleAssignService = roleAssignService;
            _logger = logger;
        }

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