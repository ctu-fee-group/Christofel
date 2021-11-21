//
//   RequirePermissionCondition.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Remora.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
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
        private readonly ICommandContext _context;
        private readonly ILogger _logger;
        private readonly ChristofelCommandPermissionResolver _permissionResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirePermissionCondition"/> class.
        /// </summary>
        /// <param name="permissionResolver">The permission resolver.</param>
        /// <param name="context">The context of the current command.</param>
        /// <param name="logger">The logger.</param>
        public RequirePermissionCondition
        (
            ChristofelCommandPermissionResolver permissionResolver,
            ICommandContext context,
            ILogger<RequirePermissionCondition> logger
        )
        {
            _logger = logger;
            _context = context;
            _permissionResolver = permissionResolver;
        }

        /// <inheritdoc />
        public async ValueTask<Result> CheckAsync
        (
            RequirePermissionAttribute attribute,
            CancellationToken ct = default
        )
        {
            var result = false;
            var roles = GetRoles();

            if (roles.HasValue)
            {
                result = await _permissionResolver.HasPermissionAsync
                (
                    _context.User.ID,
                    roles.Value,
                    attribute.Permission,
                    ct
                );
            }
            else
            {
                result = await _permissionResolver.HasPermissionAsync
                (
                    _context.User,
                    attribute.Permission,
                    ct
                );
            }

            if (!result)
            {
                _logger.LogWarning
                (
                    $"User <@{_context.User.ID.Value}> ({_context.User.Username}#{_context.User.Discriminator}) tried to execute command {GetCommandName()}, but does not have sufficient permissions"
                );
            }

            return result
                ? Result.FromSuccess()
                : new InvalidOperationException("You don't have sufficient permissions");
        }

        private string GetCommandName()
        {
            if (_context is InteractionContext interactionContext && interactionContext.Data.Name.HasValue)
            {
                return interactionContext.Data.Name.Value;
            }

            if (_context is MessageContext messageContext && messageContext.Message.Content.HasValue)
            {
                return messageContext.Message.Content.Value.Split(' ', 2).FirstOrDefault() ??
                       "(Unknown message command)";
            }

            return "(Unknown name)";
        }

        private Optional<IReadOnlyList<Snowflake>> GetRoles()
        {
            if (_context is InteractionContext interactionContext)
            {
                return interactionContext.Member.HasValue
                    ? new Optional<IReadOnlyList<Snowflake>>(interactionContext.Member.Value.Roles)
                    : default;
            }

            if (_context is MessageContext messageContext)
            {
                var partialMember = messageContext.Message.Member;
                return partialMember.HasValue
                    ? partialMember.Value.Roles
                    : default;
            }

            return default;
        }
    }
}