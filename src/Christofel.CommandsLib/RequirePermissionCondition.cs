using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Remora.Commands.Conditions;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Core;
using Remora.Results;

namespace Christofel.CommandsLib
{
    public class RequirePermissionCondition : ICondition<RequirePermissionAttribute>
    {
        private readonly ChristofelCommandPermissionResolver _permissionResolver;
        private readonly ICommandContext _context;
        private readonly ILogger _logger;

        public RequirePermissionCondition(ChristofelCommandPermissionResolver permissionResolver,
            ICommandContext context, ILogger<RequirePermissionCondition> logger)
        {
            _logger = logger;
            _context = context;
            _permissionResolver = permissionResolver;
        }

        public async ValueTask<Result> CheckAsync(RequirePermissionAttribute attribute,
            CancellationToken ct = new CancellationToken())
        {
            bool result = false;
            var roles = GetRoles();

            if (roles.HasValue)
            {
                result = await _permissionResolver.HasPermissionAsync(_context.User.ID, roles.Value,
                    attribute.Permission, ct);
            }
            else
            {
                result = await _permissionResolver.HasPermissionAsync(_context.User, attribute.Permission,
                    ct);
            }

            if (!result)
            {
                _logger.LogWarning(
                    $"User <@{_context.User.ID.Value}> ({_context.User.Username}#{_context.User.Discriminator}) tried to execute command {GetCommandName()}, but does not have sufficient permissions");
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
            else if (_context is MessageContext messageContext)
            {
                var partialMember = messageContext.Message.Member;
                return partialMember.HasValue ? partialMember.Value.Roles : default;
            }

            return default;
        }
    }
}