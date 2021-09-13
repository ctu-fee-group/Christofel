//
//   AssignRoleEntityRecoverableService.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu.Database;
using Christofel.Api.Ctu.Jobs;
using Christofel.Scheduler;
using Christofel.Scheduler.Abstractions;
using Christofel.Scheduler.Recoverable;
using Christofel.Scheduler.Triggers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Remora.Results;

namespace Christofel.Api.Services
{
    /// <summary>
    /// Service for recovering assign role entity.
    /// </summary>
    public class
        AssignRoleEntityRecoverableService : EntityJobRecoverableService<CtuAuthAssignRoleJob, CtuAuthRoleAssign>
    {
        private readonly NonConcurrentTrigger.State _ncState;
        private readonly IDbContextFactory<ApiCacheContext> _apiCacheContextFactory;
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignRoleEntityRecoverableService"/> class.
        /// </summary>
        /// <param name="guildApi">The guild api.</param>
        /// <param name="apiCacheContextFactory">The api database context factory.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="ncState">The state for non concurrent trigger.</param>
        public AssignRoleEntityRecoverableService
        (
            IDiscordRestGuildAPI guildApi,
            IDbContextFactory<ApiCacheContext> apiCacheContextFactory,
            ILogger<AssignRoleEntityRecoverableService> logger,
            NonConcurrentTrigger.State ncState
        )
        {
            _logger = logger;
            _guildApi = guildApi;
            _apiCacheContextFactory = apiCacheContextFactory;
            _ncState = ncState;
        }

        /// <inheritdoc />
        protected override async Task<Result<IReadOnlyList<CtuAuthRoleAssign>>> GetEntitiesAsync
            (CancellationToken ct = default)
        {
            await using var dbContext = _apiCacheContextFactory.CreateDbContext();
            var returnList = new List<CtuAuthRoleAssign>();
            try
            {
                var rolesToAssign = (await dbContext.AssignRoles
                        .ToListAsync(ct))
                    .GroupBy
                    (
                        x => new
                        {
                            x.UserDiscordId,
                            x.GuildDiscordId,
                        },
                        x => new
                        {
                            x.RoleId,
                            x.Add,
                        }
                    );

                foreach (var userGrouping in rolesToAssign)
                {
                    var guildId = userGrouping.Key.GuildDiscordId;
                    var userId = userGrouping.Key.UserDiscordId;
                    var assignRoles = userGrouping.Where(x => x.Add).Select(x => x.RoleId).ToArray();
                    var removeRoles = userGrouping.Where(x => !x.Add).Select(x => x.RoleId).ToArray();
                    var entity = new CtuAuthRoleAssign(userId, guildId, assignRoles, removeRoles);

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
                                    "Could not add remaining roles to user <@{UserId}> in guild {GuildId} as the user is not present in the guild. Going to remove the roles from database",
                                    userId,
                                    guildId
                                );
                                await RemoveEntityAsync(entity, ct);
                                continue;
                            default:
                                _logger.LogWarning
                                (
                                    "There was an error obtaining user from api: {Error}, but going to enqueue for addition anyway",
                                    guildMemberResult.Error.Message
                                );
                                guildMember = new GuildMember
                                (
                                    default,
                                    default,
                                    new List<Snowflake>(0),
                                    DateTimeOffset.MinValue,
                                    default,
                                    default,
                                    default
                                );
                                break;
                        }
                    }
                    else
                    {
                        guildMember = guildMemberResult.Entity;
                    }

                    var assignMissingRoles = assignRoles.Except(guildMember.Roles);
                    var removeIntersectedRoles = removeRoles.Intersect(guildMember.Roles);

                    returnList.Add
                    (
                        new CtuAuthRoleAssign
                            (userId, guildId, assignMissingRoles.ToList(), removeIntersectedRoles.ToList())
                    );
                }
            }
            catch (Exception e)
            {
                return e;
            }

            return returnList;
        }

        /// <inheritdoc />
        protected override async Task<Result> SaveEntityAsync
            (CtuAuthRoleAssign entity, CancellationToken ct = default)
        {
            await using var dbContext = _apiCacheContextFactory.CreateDbContext();

            foreach (var assignRole in entity.AddRoles)
            {
                dbContext.Add
                (
                    new AssignRole
                    {
                        Add = true,
                        RoleId = assignRole,
                        UserDiscordId = entity.UserId,
                        GuildDiscordId = entity.GuildId,
                    }
                );
            }

            foreach (var removeRole in entity.RemoveRoles)
            {
                dbContext.Add
                (
                    new AssignRole
                    {
                        Add = false,
                        RoleId = removeRole,
                        UserDiscordId = entity.UserId,
                        GuildDiscordId = entity.GuildId,
                    }
                );
            }

            try
            {
                await dbContext.SaveChangesAsync(ct);
            }
            catch (Exception e)
            {
                return e;
            }

            return Result.FromSuccess();
        }

        /// <inheritdoc />
        protected override async Task<Result> RemoveEntityAsync
            (CtuAuthRoleAssign entity, CancellationToken ct = default)
        {
            await using var dbContext = _apiCacheContextFactory.CreateDbContext();
            var entries = await dbContext.AssignRoles
                .Where(x => x.UserDiscordId == entity.UserId && x.GuildDiscordId == entity.GuildId)
                .ToListAsync(ct);

            dbContext.RemoveRange(entries);
            try
            {
                await dbContext.SaveChangesAsync(ct);
            }
            catch (Exception e)
            {
                return e;
            }

            return Result.FromSuccess();
        }

        /// <inheritdoc />
        protected override IJobData CreateJob
            (CtuAuthRoleAssign entity)
            => new TypedJobData<CtuAuthAssignRoleJob>(new JobKey("Auth", $"Assign roles to <@{entity.UserId}>"))
                .AddData("Data", entity);

        /// <inheritdoc />
        protected override ITrigger CreateTrigger(IJobData job)
            => new NonConcurrentTrigger(new SimpleTrigger(), _ncState);
    }
}