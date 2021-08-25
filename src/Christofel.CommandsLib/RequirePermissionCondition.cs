using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Remora.Commands.Conditions;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
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
            if (_context is not InteractionContext interactionContext)
            {
                return new InvalidOperationException("Cannot execute the command outside of itneractions");
            }

            bool result = false;
            if (interactionContext.Member.HasValue)
            {
                result = await _permissionResolver.HasPermissionAsync(interactionContext.Member.Value,
                    attribute.Permission, ct);
            }
            else
            {
                result = await _permissionResolver.HasPermissionAsync(interactionContext.User, attribute.Permission,
                    ct);
            }

            if (!result)
            {
                _logger.LogWarning(
                    $"User <@{interactionContext.User.ID.Value}> ({interactionContext.User.Username}#{interactionContext.User.Discriminator}) tried to execute command {interactionContext.Data.Name}, but does not have sufficient permissions");
            }

            return result
                ? Result.FromSuccess()
                : new InvalidOperationException("You don't have sufficient permissions");
        }
    }
}