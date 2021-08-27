using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Remora.Discord.Core;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Tasks
{
    public class AssignRolesAuthTask : IAuthTask
    {
        private readonly ILogger _logger;
        private readonly CtuAuthRoleAssignService _roleAssignService;

        public AssignRolesAuthTask(ILogger<CtuAuthProcess> logger,
            CtuAuthRoleAssignService roleAssignService)
        {
            _roleAssignService = roleAssignService;
            _logger = logger;
        }

        public async Task<Result> ExecuteAsync(IAuthData data, CancellationToken ct = default)
        {
            _logger.LogDebug(
                $"Going to enqueue role assignments for user <@{data.DbUser.DiscordId}>. Add roles: {string.Join(", ", data.Roles.AddRoles.Select(x => x.RoleId))}. Remove roles: {string.Join(", ", data.Roles.SoftRemoveRoles.Select(x => x.RoleId))}");

            var assignRoles = data.Roles.AddRoles.Select(x => x.RoleId).ToArray();
            var removeRoles = data.Roles.SoftRemoveRoles.Select(x => x.RoleId).Except(assignRoles).ToArray();

            // Save to cache
            await _roleAssignService.SaveRoles(
                data.DbUser.DiscordId,
                data.GuildId,
                assignRoles,
                removeRoles);

            _roleAssignService.EnqueueRoles(
                data.GuildUser,
                data.DbUser.DiscordId,
                data.GuildId,
                assignRoles,
                removeRoles);

            return Result.FromSuccess();
        }
    }
}