//
//   RestoreAssignRolesService.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu;
using Christofel.Api.Ctu.Jobs;
using Christofel.Scheduler.Abstractions;
using Christofel.Scheduler.Recoverable;
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
        private readonly IScheduler _scheduler;
        private readonly IJobRecoverService<CtuAuthAssignRoleJob> _roleAssignService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestoreAssignRolesService"/> class.
        /// </summary>
        /// <param name="roleAssignService">The service for assigning roles.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="scheduler">The scheduler to schedule tasks with.</param>
        public RestoreAssignRolesService
        (
            IJobRecoverService<CtuAuthAssignRoleJob> roleAssignService,
            ILogger<RestoreAssignRolesService> logger,
            IScheduler scheduler
        )
        {
            _logger = logger;
            _scheduler = scheduler;
            _roleAssignService = roleAssignService;
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var restoredResult = await _roleAssignService.RecoverJobsAsync(_scheduler, cancellationToken);
            if (restoredResult.IsSuccess)
            {
                _logger.LogInformation
                    ("Restored role assignments for {restoredCount} members.", restoredResult.Entity.Count);
            }
            else
            {
                _logger.LogError("Could not restore assigning roles process {Error}", restoredResult.Error.Message);
            }
        }

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken) =>
            Task.CompletedTask; // Nothing is needed to stop
    }
}