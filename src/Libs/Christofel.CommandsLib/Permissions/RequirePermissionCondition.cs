//
//   RequirePermissionCondition.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Extensions;
using Christofel.Helpers.Permissions;
using Microsoft.Extensions.Logging;
using Remora.Commands.Conditions;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.CommandsLib.Permissions
{
    /// <summary>
    /// Condition for <see cref="RequirePermissionAttribute"/> that makes sure
    /// only users with the given permission can execute the command.
    /// </summary>
    public class RequirePermissionCondition : ICondition<RequirePermissionAttribute>
    {
        private readonly IOperationContext _context;
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly ILogger _logger;
        private readonly ChristofelCommandPermissionResolver _permissionResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirePermissionCondition"/> class.
        /// </summary>
        /// <param name="permissionResolver">The permission resolver.</param>
        /// <param name="context">The context of the current command.</param>
        /// <param name="guildApi">The guild api.</param>
        /// <param name="logger">The logger.</param>
        public RequirePermissionCondition
        (
            ChristofelCommandPermissionResolver permissionResolver,
            IOperationContext context,
            IDiscordRestGuildAPI guildApi,
            ILogger<RequirePermissionCondition> logger
        )
        {
            _logger = logger;
            _context = context;
            _guildApi = guildApi;
            _permissionResolver = permissionResolver;
        }

        /// <inheritdoc />
        public async ValueTask<Result> CheckAsync
        (
            RequirePermissionAttribute attribute,
            CancellationToken ct = default
        )
        {
            var rolesResult = await GetRolesAsync(ct);
            if (!rolesResult.IsDefined(out var roles))
            {
                return Result.FromError(rolesResult);
            }

            if (!_context.TryGetUserID(out var userId))
            {
                return new GenericError($"Could not obtain user id in {nameof(RequirePermissionCondition)}.");
            }

            var result = await _permissionResolver.HasPermissionAsync
            (
                userId.Value,
                roles.Value ?? Array.Empty<Snowflake>(),
                attribute.Permission,
                ct
            );

            if (!result)
            {
                _logger.LogWarning
                (
                    $"User <@{userId}> ({_context.GetUserDiscordHandleOrDefault()}) tried to execute command {GetCommandName()}, but does not have sufficient permissions"
                );
            }

            return result
                ? Result.FromSuccess()
                : new InvalidOperationException("You don't have sufficient permissions");
        }

        private string GetCommandName()
        {
            if (_context is InteractionContext interactionContext &&
                interactionContext.Interaction.Data.TryGet(out var interactionData) &&
                interactionData.TryPickT0(out var data, out _) &&
                !string.IsNullOrEmpty(data.Name))
            {
                return data.Name;
            }

            if (_context is MessageContext messageContext && messageContext.Message.Content.HasValue)
            {
                return messageContext.Message.Content.Value.Split(' ', 2).FirstOrDefault() ??
                       "(Unknown message command)";
            }

            return "(Unknown name)";
        }

        private async Task<Result<Optional<IReadOnlyList<Snowflake>>>> GetRolesAsync(CancellationToken ct)
        {
            if (!_context.TryGetUserID(out var userId))
            {
                return new GenericError("Could not get user id from context.");
            }

            if (_context is InteractionContext interactionContext)
            {
                return interactionContext.Interaction.Member.HasValue
                    ? new Optional<IReadOnlyList<Snowflake>>(interactionContext.Interaction.Member.Value.Roles)
                    : default;
            }

            if (_context is MessageContext && _context.TryGetGuildID(out var guildId))
            {
                var memberResult = await _guildApi.GetGuildMemberAsync(guildId.Value, userId.Value, ct);
                if (!memberResult.IsDefined(out var member))
                {
                    return Result<Optional<IReadOnlyList<Snowflake>>>.FromError(memberResult);
                }
                return new Optional<IReadOnlyList<Snowflake>>(member.Roles);
            }

            return default;
        }
    }
}