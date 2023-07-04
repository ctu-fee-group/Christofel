//
//  RemoveLinkedRolesAuthTask.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Tasks;

/// <summary>
/// Task for removing roles of linked users.
/// </summary>
public class RemoveLinkedRolesAuthTask : IAuthTask
{
    private readonly ILogger _logger;
    private readonly IDiscordRestGuildAPI _guildApi;
    private readonly BotOptions _botOptions;
    private readonly CtuAuthRoleAssignService _roleAssignService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveLinkedRolesAuthTask"/> class.
    /// </summary>
    /// <param name="botOptions">The options of the application.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="guildApi">The guild api.</param>
    /// <param name="roleAssignService">The service for processing roles assignment.</param>
    public RemoveLinkedRolesAuthTask
    (
        IOptions<BotOptions> botOptions,
        ILogger<RemoveLinkedRolesAuthTask> logger,
        IDiscordRestGuildAPI guildApi,
        CtuAuthRoleAssignService roleAssignService
    )
    {
        _botOptions = botOptions.Value;
        _roleAssignService = roleAssignService;
        _logger = logger;
        _guildApi = guildApi;
    }

    /// <inheritdoc />
    public async Task<Result> ExecuteAsync(IAuthData data, CancellationToken ct = default)
    {
        foreach (var linkedAccount in data.UnapprovedLinkedAccounts)
        {
            var guildMemberResult = await _guildApi.GetGuildMemberAsync
                (DiscordSnowflake.New(_botOptions.GuildId), linkedAccount.DiscordId, ct);

            if (!guildMemberResult.IsDefined(out var guildMember))
            {
                _logger.LogResultError(guildMemberResult, "Could not find linked member");
                continue;
            }

            var removeRoles = await data.DbContext.RoleAssignments
                .AsNoTracking()
                .Select(x => x.RoleId)
                .Where(x => guildMember.Roles.Contains(x))
                .ToArrayAsync(ct);

            if (removeRoles.Length == 0)
            {
                return Result.FromSuccess();
            }

            foreach (var unapprovedLinkedUser in data.UnapprovedLinkedAccounts)
            {
                // Save to cache
                try
                {
                    await _roleAssignService.SaveRoles
                    (
                        unapprovedLinkedUser.DiscordId,
                        data.GuildId,
                        Array.Empty<Snowflake>(),
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
                    guildMember,
                    unapprovedLinkedUser.DiscordId,
                    data.GuildId,
                    Array.Empty<Snowflake>(),
                    removeRoles
                );
            }
        }
        return Result.FromSuccess();
    }
}