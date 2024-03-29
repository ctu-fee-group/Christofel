//
//   RestoreAssignRolesService.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.CtuAuth;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Christofel.Api.Services
{
    /// <summary>
    /// Restores process of assigning roles where it was left.
    /// </summary>
    public class RestoreAssignRolesService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly CtuAuthRoleAssignService _roleAssignService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestoreAssignRolesService"/> class.
        /// </summary>
        /// <param name="roleAssignService">The service for assigning roles.</param>
        /// <param name="logger">The logger.</param>
        public RestoreAssignRolesService
            (CtuAuthRoleAssignService roleAssignService, ILogger<RestoreAssignRolesService> logger)
        {
            _logger = logger;
            _roleAssignService = roleAssignService;
        }

        /// <inheritdoc />
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

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken) =>
            Task.CompletedTask; // Nothing is needed to stop
    }
}