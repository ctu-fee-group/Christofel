//
//   CtuAuthRoleAssignService.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu.Database;
using Christofel.Api.Ctu.JobQueue;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Remora.Results;

namespace Christofel.Api.Ctu
{
    /// <summary>
    /// Service used for storing roles to be assigned in database,
    /// enqueues pending data from the database when needed
    /// </summary>
    public class CtuAuthRoleAssignService
    {
        private readonly IDbContextFactory<ApiCacheContext> _dbContextFactory;
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly ILogger _logger;
        private readonly IJobQueue<CtuAuthRoleAssign> _roleAssignProcessor;

        public CtuAuthRoleAssignService
        (
            IDbContextFactory<ApiCacheContext> dbContextFactory,
            IDiscordRestGuildAPI guildApi,
            IJobQueue<CtuAuthRoleAssign> roleAssignProcessor,
            ILogger<CtuAuthRoleAssignService> logger
        )
        {
            _roleAssignProcessor = roleAssignProcessor;
            _guildApi = guildApi;
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        /// <summary>
        /// Enqueues roles to be assigned and/or removed from the given user
        /// </summary>
        /// <param name="guildMember">Loaded guild member with roles that are currently</param>
        /// <param name="userId">Id of the user/member the roles should be assigned/removed from</param>
        /// <param name="guildId">Id of the guild where the roles should be assigned</param>
        /// <param name="assignRoles">What roles should be assigned to the member</param>
        /// <param name="removeRoles">What roles should be removed from the member</param>
        public void EnqueueRoles
        (
            IGuildMember guildMember,
            Snowflake userId,
            Snowflake guildId,
            IReadOnlyList<Snowflake> assignRoles,
            IReadOnlyList<Snowflake> removeRoles
        )
        {
            var assignMissingRoles = assignRoles.Except(guildMember.Roles);
            var removeIntersectedRoles = removeRoles.Intersect(guildMember.Roles);

            _roleAssignProcessor.EnqueueJob
            (
                new CtuAuthRoleAssign
                (
                    userId,
                    guildId,
                    assignMissingRoles.ToArray(),
                    removeIntersectedRoles.ToArray(),
                    () => Task.Run(async () => await RemoveRoles(userId, guildId))
                )
            );
        }

        /// <summary>
        /// Save given roles to database to be able to retrieve them in case the bot is stopped
        /// </summary>
        /// <param name="userId">What user should the roles be assigned to</param>
        /// <param name="guildId">What guild are the roles in</param>
        /// <param name="assignRoles">What roles should be assigned</param>
        /// <param name="removeRoles">What roles should be deleted</param>
        /// <returns>Task saving the information to the database</returns>
        public async Task SaveRoles
        (
            Snowflake userId,
            Snowflake guildId,
            IReadOnlyList<Snowflake> assignRoles,
            IReadOnlyList<Snowflake> removeRoles,
            CancellationToken ct = default
        )
        {
            await using var dbContext = _dbContextFactory.CreateDbContext();

            foreach (var assignRole in assignRoles)
            {
                dbContext.Add
                (
                    new AssignRole
                    {
                        Add = true, RoleId = assignRole, UserDiscordId = userId, GuildDiscordId = guildId,
                    }
                );
            }

            foreach (var removeRole in removeRoles)
            {
                dbContext.Add
                (
                    new AssignRole
                    {
                        Add = false, RoleId = removeRole, UserDiscordId = userId, GuildDiscordId = guildId,
                    }
                );
            }

            await dbContext.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Remove roles of the given user from the database as they are no longer needed
        /// </summary>
        /// <param name="userId">What user should be removed from the database</param>
        /// <param name="guildId">What guild is the user in</param>
        /// <returns>Task removing the information from the database</returns>
        public async Task RemoveRoles(Snowflake userId, Snowflake guildId, CancellationToken ct = default)
        {
            try
            {
                // TODO: use batch delete
                await using var dbContext = _dbContextFactory.CreateDbContext();
                var entries = await dbContext.AssignRoles
                    .Where(x => x.UserDiscordId == userId && x.GuildDiscordId == guildId)
                    .ToListAsync(ct);
                //.BatchDeleteAsync(ct);

                dbContext.RemoveRange(entries);
                await dbContext.SaveChangesAsync(ct);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not remove roles of user {UserId} from the database", userId);
            }
        }

        /// <summary>
        /// Enqueue all roles to the processor
        /// </summary>
        /// <remarks>
        /// Enqueues roles from cache database,
        /// users who didn't get roles assigned in previous run,
        /// will get roles assigned in this one.
        /// </remarks>
        /// <returns>Number of users that were enqueued for role addition</returns>
        public async Task<uint> EnqueueRemainingRoles(CancellationToken ct = default)
        {
            await using var dbContext = _dbContextFactory.CreateDbContext();
            var rolesToAssign = (await dbContext.AssignRoles
                    .ToListAsync(ct))
                .GroupBy(x => new { x.UserDiscordId, x.GuildDiscordId }, x => new { x.RoleId, x.Add });

            uint remaining = 0;
            foreach (var userGrouping in rolesToAssign)
            {
                var guildId = userGrouping.Key.GuildDiscordId;
                var userId = userGrouping.Key.UserDiscordId;
                var assignRoles = userGrouping.Where(x => x.Add).Select(x => x.RoleId).ToArray();
                var removeRoles = userGrouping.Where(x => !x.Add).Select(x => x.RoleId).ToArray();

                var guildMemberResult =
                    await _guildApi.GetGuildMemberAsync(guildId, userId, ct);
                IGuildMember guildMember;
                if (!guildMemberResult.IsSuccess)
                {
                    switch (guildMemberResult.Error)
                    {
                        case NotFoundError:
                            _logger.LogWarning
                            (
                                $"Could not add remaining roles to user <@{userId}> in guild {guildId} as the user is not present in the guild. Going to remove the roles from database"
                            );
                            await RemoveRoles(userId, guildId, ct);
                            continue;
                        default:
                            _logger.LogWarning
                            (
                                "There was an error obtaining user from api: {Error}, but going to enqueue for addition anyway",
                                guildMemberResult.Error.Message
                            );
                            guildMember = new GuildMember
                            (
                                default, default, new List<Snowflake>(0),
                                DateTimeOffset.MinValue, default, default, default
                            );
                            break;
                    }
                }
                else
                {
                    guildMember = guildMemberResult.Entity;
                }

                remaining++;
                EnqueueRoles(guildMember, userId, guildId, assignRoles, removeRoles);
            }

            return remaining;
        }
    }
}