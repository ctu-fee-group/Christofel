using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Implementations.ReadOnlyDatabase;
using Christofel.BaseLib.Plugins;
using Christofel.Management.Database;
using Christofel.Management.Database.Models;
using Christofel.Plugins.Runtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Christofel.Management.Slowmode
{
    public class SlowmodeAutorestore : IStartable, IStoppable
    {
        private readonly IDbContextFactory<ManagementContext> _dbContextFactory;
        private readonly SlowmodeService _slowmodeService;
        private readonly ILogger _logger;

        public SlowmodeAutorestore(IDbContextFactory<ManagementContext> dbContextFactory,
            SlowmodeService slowmodeService, ILogger<SlowmodeAutorestore> logger)
        {
            _slowmodeService = slowmodeService;
            _logger = logger;
            _dbContextFactory = dbContextFactory;
        }

        public async Task StartAsync(CancellationToken token = new CancellationToken())
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
                        _logger.LogError("Could not disable temporal slowmode that should've ended in the past {Error}",
                            result.Error.Message);
                    }
                    else
                    {
                        removedCount++;
                    }

                    dbContext.Remove(matchingSlowmode);
                }
                else
                {
                    _slowmodeService.RegisterDisableHandler(matchingSlowmode);
                    registeredCount++;
                }
            }

            await dbContext.SaveChangesAsync(token);

            if (registeredCount > 0)
            {
                _logger.LogInformation(
                    "Restored {RegisteredCount} slowmode deferred tasks",
                    registeredCount);
            }

            if (removedCount > 0)
            {
                _logger.LogInformation(
                    "Removed {RemovedCount} old temporal slowmodes from database and disabled slowmode in these channels",
                    removedCount);
            }
        }

        public Task StopAsync(CancellationToken token = new CancellationToken())
        {
            int canceled = _slowmodeService.CancelAllDisableHandlers();

            if (canceled > 0)
            {
                _logger.LogWarning(
                    "Canceled {CanceledCount} temporal slowmode deferred tasks. These tasks will be restored when Management plugin will be attached again",
                    canceled);
            }

            return Task.CompletedTask;
        }
    }
}