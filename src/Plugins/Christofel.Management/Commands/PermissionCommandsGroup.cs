//
//   PermissionCommandsGroup.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Christofel.BaseLib.Extensions;
using Christofel.CommandsLib.Extensions;
using Christofel.CommandsLib.Permissions;
using Christofel.Common.Database;
using Christofel.Common.Database.Models;
using Christofel.Common.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace Christofel.Management.Commands
{
    /// <summary>
    /// Command group that handles /permissions commands.
    /// </summary>
    [Group("permissions")]
    [Description("Manage user and role permissions")]
    [Ephemeral]
    [RequirePermission("management.permissions")]
    public class PermissionCommandsGroup : CommandGroup
    {
        private readonly ChristofelBaseContext _dbContext;
        private readonly FeedbackService _feedbackService;
        private readonly ILogger<PermissionCommandsGroup> _logger;
        private readonly IPermissionService _permissions;

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionCommandsGroup"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="feedbackService">The feedback service.</param>
        /// <param name="dbContext">The christofel base database context.</param>
        /// <param name="permissions">The service for permissions.</param>
        public PermissionCommandsGroup
        (
            ILogger<PermissionCommandsGroup> logger,
            FeedbackService feedbackService,
            ChristofelBaseContext dbContext,
            IPermissionService permissions
        )
        {
            _dbContext = dbContext;
            _feedbackService = feedbackService;
            _logger = logger;
            _permissions = permissions;
        }

        /// <summary>
        /// Handles /permissions grant command.
        /// </summary>
        /// <remarks>
        /// Assigns permission to the given role or member.
        /// </remarks>
        /// <param name="permission">The permission to assign.</param>
        /// <param name="entity">The entity to assign the permission to. May be role or guild member.</param>
        /// <returns>A result that may not have succeeded.</returns>
        [Command("grant")]
        [RequirePermission("management.permissions.grant")]
        [Description("Grant specified permission to user or role. Specify either user or role only")]
        public async Task<Result> HandleGrant
        (
            [Description("Permission to grant to the user or role")]
            string permission,
            [Description("Entity (user or role) to assign permission to")] [DiscordTypeHint(TypeHint.Mentionable)]
            OneOf<IPartialGuildMember, IRole> entity
        )
        {
            PermissionAssignment assignment = new PermissionAssignment
            {
                PermissionName = permission,
                Target = entity.ToDiscordTarget(),
            };

            try
            {
                _dbContext.Add(assignment);
                await _dbContext.SaveChangesAsync(CancellationToken);
                var feedbackResult = await _feedbackService.SendContextualSuccessAsync
                    ("Permission granted. Refresh will be needed for it to take full effect.", ct: CancellationToken);

                return feedbackResult.IsSuccess
                    ? Result.FromSuccess()
                    : Result.FromError(feedbackResult);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not save the permission");
                var feedbackResult =
                    await _feedbackService.SendContextualErrorAsync
                    (
                        "Permission could not be saved to the database",
                        ct: CancellationToken
                    );

                return feedbackResult.IsSuccess
                    ? Result.FromSuccess()
                    : Result.FromError(feedbackResult);
            }
        }

        /// <summary>
        /// Handles /permissions revoke.
        /// </summary>
        /// <param name="permission">The permission to remove from the entity.</param>
        /// <param name="entity">The entity to remove the permission from.</param>
        /// <returns>A result that may not have succeeded.</returns>
        [Command("revoke")]
        [RequirePermission("management.permissions.revoke")]
        [Description
            ("Revoke specified permission. Specify either user or role only. Exact permission must be specified")]
        public async Task<Result> HandleRevoke
        (
            [Description("Permission to revoke from the user or role")]
            string permission,
            [Description("Entity (user or role) to assign permission to")] [DiscordTypeHint(TypeHint.Mentionable)]
            OneOf<IPartialGuildMember, IRole> entity
        )
        {
            Result<IReadOnlyList<IMessage>> feedbackResult;
            try
            {
                DiscordTarget target = entity.ToDiscordTarget();

                IQueryable<PermissionAssignment> assignments = _dbContext.Permissions
                    .AsQueryable()
                    .WhereTargetEquals(target)
                    .Where(x => x.PermissionName == permission);

                var deleted = false;
                await foreach (PermissionAssignment assignment in assignments.AsAsyncEnumerable()
                    .WithCancellation(CancellationToken))
                {
                    deleted = true;
                    _dbContext.Remove(assignment);
                }

                if (deleted)
                {
                    await _dbContext.SaveChangesAsync(CancellationToken);
                    feedbackResult =
                        await _feedbackService.SendContextualSuccessAsync("Permission revoked", ct: CancellationToken);
                }
                else
                {
                    feedbackResult =
                        await _feedbackService.SendContextualErrorAsync("Could not find that permission assignment");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not save the permission");
                feedbackResult =
                    await _feedbackService.SendContextualErrorAsync("Could not save the permission");
            }

            return feedbackResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(feedbackResult);
        }

        /// <summary>
        /// Handles /permissions list.
        /// </summary>
        /// <returns>A result that may not have succeeded.</returns>
        [Command("list")]
        [Description("Show list of all permissions that are currently loaded in Christofel")]
        [RequirePermission("management.permissions.list")]
        public async Task<Result> HandleList()
        {
            string response = "List of all permissions from attached plugins:\n";
            response += string.Join
            (
                '\n',
                _permissions.Permissions.Select
                (
                    x =>
                        $@"  - **{x.PermissionName}** - {x.DisplayName} - {x.Description}"
                )
            );

            var feedbackResult =
                await _feedbackService.SendContextualSuccessAsync(response, ct: CancellationToken);

            return feedbackResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(feedbackResult);
        }

        /// <summary>
        /// Handles /permissions show.
        /// </summary>
        /// <param name="entity">The entity to show permissions of.</param>
        /// <returns>A result that may not have succeeded.</returns>
        [Command("show")]
        [Description("Show permissions of role or user. For users their role permissions will be shown as well")]
        [RequirePermission("management.permissions.show")]
        public async Task<Result> HandleShow
        (
            [Description("Show permissions of entity (user or role)")] [DiscordTypeHint(TypeHint.Mentionable)]
            OneOf<IPartialGuildMember, IRole> entity
        )
        {
            var targets = entity.GetAllDiscordTargets();

            Result<IReadOnlyList<IMessage>> feedbackResult;
            try
            {
                List<string> permissionAssignments = (await _dbContext.Permissions
                        .AsNoTracking()
                        .WhereTargetAnyOf(targets)
                        .ToListAsync(CancellationToken))
                    .GroupBy(x => x.Target, new DiscordTargetComparer())
                    .Select
                    (
                        grouping =>
                            $@"Permission of {grouping.Key.GetMentionString()}:" +
                            "\n" +
                            string.Join
                            (
                                '\n',
                                grouping.Select(perm => $@"  - **{perm.PermissionName.Replace("*", "\\*")}**")
                            )
                    )
                    .ToList();

                string response;
                if (permissionAssignments.Count == 0)
                {
                    response = "Specified target does not have any permissions";
                }
                else
                {
                    response = "Showing all permissions: " + string.Join('\n', permissionAssignments);
                }

                feedbackResult = await _feedbackService.SendContextualSuccessAsync(response, ct: CancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not get user/role permission from the database");
                feedbackResult =
                    await _feedbackService.SendContextualErrorAsync
                        ("Could not get user/role permission from the database");
            }

            return feedbackResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(feedbackResult);
        }

        private class DiscordTargetComparer : IEqualityComparer<DiscordTarget>
        {
            public bool Equals(DiscordTarget? x, DiscordTarget? y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (ReferenceEquals(x, null))
                {
                    return false;
                }

                if (ReferenceEquals(y, null))
                {
                    return false;
                }

                if (x.GetType() != y.GetType())
                {
                    return false;
                }

                return x.DiscordId == y.DiscordId && x.GuildId == y.GuildId && x.TargetType == y.TargetType;
            }

            public int GetHashCode(DiscordTarget obj) => HashCode.Combine
                (obj.DiscordId, obj.GuildId, (int)obj.TargetType);
        }
    }
}