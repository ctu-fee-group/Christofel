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
        private readonly CtuAuthRoleAssignProcessor _roleAssignProcessor;
        private readonly ILogger _logger;
        
        public AssignRolesAuthTask(ILogger<CtuAuthProcess> logger, CtuAuthRoleAssignProcessor roleAssignProcessor)
        {
            _logger = logger;
            _roleAssignProcessor = roleAssignProcessor;
        }

        public Task<Result> ExecuteAsync(IAuthData data, CancellationToken ct = default)
        {
            _logger.LogDebug(
                $"Going to enqueue role assignments for user <@{data.DbUser.DiscordId}>. Add roles: {string.Join(", ", data.Roles.AddRoles.Select(x => x.RoleId))}. Remove roles: {string.Join(", ", data.Roles.SoftRemoveRoles.Select(x => x.RoleId))}");

            var assignRoles = data.Roles.AddRoles.ToList();
            var removeRoles = data.Roles.SoftRemoveRoles.Except(assignRoles).ToList();
            
            _roleAssignProcessor.EnqueueAssignJob(
                data.GuildUser,
                new Snowflake(data.DbUser.DiscordId),
                new Snowflake(data.GuildId),
                assignRoles,
                removeRoles
            );

            return Task.FromResult(Result.FromSuccess());
        }
    }
}