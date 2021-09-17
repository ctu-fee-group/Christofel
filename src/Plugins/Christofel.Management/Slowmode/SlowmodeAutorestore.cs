//
//   SlowmodeAutorestore.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Management.Database;
using Christofel.Plugins.Runtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Christofel.Management.Slowmode
{
    /// <summary>
    /// Service for auto restoring slowmode disable tasks on startup of the plugin.
    /// </summary>
    public class SlowmodeAutorestore : IStartable, IStoppable
    {
        private readonly IDbContextFactory<ManagementContext> _dbContextFactory;
        private readonly ILogger _logger;
        private readonly SlowmodeService _slowmodeService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlowmodeAutorestore"/> class.
        /// </summary>
        /// <param name="dbContextFactory">The management database context factory.</param>
        /// <param name="slowmodeService">The service for managing slowmodes.</param>
        /// <param name="logger">The logger.</param>
        public SlowmodeAutorestore
        (
            IDbContextFactory<ManagementContext> dbContextFactory,
            SlowmodeService slowmodeService,
            ILogger<SlowmodeAutorestore> logger
        )
        {
            _slowmodeService = slowmodeService;
            _logger = logger;
            _dbContextFactory = dbContextFactory;
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken token = default)
        {
            await using var dbContext = _dbContextFactory.CreateDbContext();
            var matchingSlowmodes = dbContext.TemporalSlowmodes;

            uint removedCount = 0, registeredCount = 0;
            await foreach (var matchingSlowmode in matchingSlowmodes)
            {
                if (matchingSlowmode.DeactivationDate < DateTime.Now)
                {
                    var result = await _slowmodeService.DisableSlowmodeAsync(matchingSlowmode.ChannelId, token);

                    if (!result.IsSuccess)
                    {
                        _logger.LogError
                        (
                            "Could not disable temporal slowmode that should've ended in the past {Error}",
                            result.Error.Message
                        );
                    }
                    else
                    {
                        removedCount++;
                    }

                    dbContext.Remove(matchingSlowmode);
                }
                else
                {
                    await _slowmodeService.RegisterDisableHandlerAsync(matchingSlowmode, token);
                    registeredCount++;
                }
            }

            await dbContext.SaveChangesAsync(token);

            if (registeredCount > 0)
            {
                _logger.LogInformation
                (
                    "Restored {RegisteredCount} slowmode deferred tasks",
                    registeredCount
                );
            }

            if (removedCount > 0)
            {
                _logger.LogInformation
                (
                    "Removed {RemovedCount} old temporal slowmodes from database and disabled slowmode in these channels",
                    removedCount
                );
            }
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken token = default)
        {
            var canceled = _slowmodeService.CancelAllDisableHandlers();

            if (canceled > 0)
            {
                _logger.LogWarning
                (
                    "Canceled {CanceledCount} temporal slowmode deferred tasks. These tasks will be restored when Management plugin will be attached again",
                    canceled
                );
            }

            return Task.CompletedTask;
        }
    }
}