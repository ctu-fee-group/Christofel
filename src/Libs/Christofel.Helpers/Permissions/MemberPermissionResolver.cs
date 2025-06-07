//
//   MemberPermissionResolver.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Extensions;
using Christofel.Common.Database.Models;
using Christofel.Common.Database.Models.Enums;
using Christofel.Common.Permissions;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.Helpers.Permissions;

/// <summary>
/// Resolves member permissions.
/// </summary>
public class MemberPermissionResolver
{
    private readonly IPermissionsResolver _permissionsResolver;
    private readonly IDiscordRestGuildAPI _guildApi;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberPermissionResolver"/> class.
    /// </summary>
    /// <param name="permissionsResolver">The permissions resolver.</param>
    /// <param name="guildApi">The guild id.</param>
    public MemberPermissionResolver(IPermissionsResolver permissionsResolver, IDiscordRestGuildAPI guildApi)
    {
        _permissionsResolver = permissionsResolver;
        _guildApi = guildApi;

    }

    /// <summary>
    ///  Checks whether the specified member has the given permission.
    /// </summary>
    /// <param name="permission">The permission string.</param>
    /// <param name="userId">The id of the user.</param>
    /// <param name="guildId">The id of the guild.</param>
    /// <param name="guildMember">The optional guild member.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A result that signals if the user has permission or an error.</returns>
    public async Task<Result<bool>> HasPermissionAsync
    (
        string permission,
        Snowflake userId,
        Optional<Snowflake> guildId = default,
        Optional<IGuildMember> guildMember = default,
        CancellationToken ct = default
    )
    {
        if (guildMember.IsDefined(out var member))
        {
            return await _permissionsResolver.AnyHasPermissionAsync
                (permission, member.GetAllDiscordTargets(userId), ct);
        }

        if (guildId.IsDefined(out var guild))
        {
            var guildMemberResult = await _guildApi.GetGuildMemberAsync(guild, userId, ct);
            if (guildMemberResult.IsDefined(out var fetchedMember))
            {
                return await _permissionsResolver.AnyHasPermissionAsync
                    (permission, fetchedMember.GetAllDiscordTargets(userId), ct);
            }
        }

        return await _permissionsResolver.HasPermissionAsync
            (permission, new DiscordTarget(userId, TargetType.User), ct);
    }
}