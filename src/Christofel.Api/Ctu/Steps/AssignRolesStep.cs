using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Christofel.Api.Ctu.Steps
{
    /// <summary>
    /// Enqueue roles to queue for addition or removal on Discord
    /// </summary>
    public class AssignRolesStep : CtuAuthStep
    {
        private readonly CtuAuthRoleAssignProcessor _roleAssignProcessor;

        public AssignRolesStep(ILogger<CtuAuthProcess> logger, CtuAuthRoleAssignProcessor roleAssignProcessor) :
            base(logger)
        {
            _roleAssignProcessor = roleAssignProcessor;
        }

        protected override Task<bool> HandleStep(CtuAuthProcessData data)
        {
            _logger.LogDebug(
                $"Going to enqueue role assignments for user {data.GuildUser}. Add roles: {string.Join(", ", data.Roles.AddRoles.Select(x => x.RoleId))}. Remove roles: {string.Join(", ", data.Roles.RemoveRoles.Select(x => x.RoleId))}");
            _roleAssignProcessor.EnqueueAssignJob(
                data.GuildUser,
                data.Roles.AddRoles.ToList(),
                data.Roles.RemoveRoles.ToList()
            );

            return Task.FromResult(true);
        }
    }
}