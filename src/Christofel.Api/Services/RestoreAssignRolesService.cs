using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Christofel.Api.Services
{
    /// <summary>
    /// Restores process of assigning roles where it was left
    /// </summary>
    public class RestoreAssignRolesService : IHostedService
    {
        private CtuAuthRoleAssignService _roleAssignService;
        private ILogger _logger;
        
        public RestoreAssignRolesService(CtuAuthRoleAssignService roleAssignService, ILogger<RestoreAssignRolesService> logger)
        {
            _logger = logger;
            _roleAssignService = roleAssignService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var restoredCount = await _roleAssignService.EnqueueRemainingRoles(cancellationToken);
                _logger.LogInformation("Restored role assignments for {restoredCount} members.", restoredCount);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not restore assigning roles process");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Nothing is needed to stop
            return Task.CompletedTask;
        }
    }
}