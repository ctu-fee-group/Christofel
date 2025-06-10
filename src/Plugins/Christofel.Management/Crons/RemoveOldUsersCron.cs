//
//   RemoveOldUsersCron.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Christofel.Common.Database;
using Christofel.Helpers.Cron;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.Management.Crons;

/// <summary>
/// Removes User entries from database older than two days, where the
/// AuthenticatedAt has not been set. These are unfinished authentication
/// entries. They are unlikely to be finished in the future, since longer time
/// has passed since their creation.
/// This task is intended to run weekly.
/// </summary>
public class RemoveOldUsersCron : SimpleCronJob
{
    private readonly IDbContextFactory<ChristofelBaseContext> _dbContextFactory;
    private readonly ILogger<RemoveOldUsersCron> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveOldUsersCron"/> class.
    /// </summary>
    /// <param name="dbContextFactory">Factory for creating the ChristofelBaseContext.</param>
    /// <param name="logger">Logger to log information and errors with.</param>
    public RemoveOldUsersCron
        (
            IDbContextFactory<ChristofelBaseContext> dbContextFactory,
            ILogger<RemoveOldUsersCron> logger
        )
        : base
        (
            "RemoveOldUsers",
            logger,
            TimeSpan.FromDays(7)
        )
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override async Task<IResult> ProcessAsync(bool manual)
    {
        using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
        {
            var beforeTwoDays = DateTime.Now.Subtract(TimeSpan.FromDays(2));
            var count = 0;

            foreach (var userToDelete in dbContext.Users
                .Where(x => x.AuthenticatedAt == null && x.CreatedAt < beforeTwoDays))
            {
                dbContext.Remove(userToDelete);
                count++;
            }

            await dbContext.SaveChangesAsync();

            _logger.LogInformation
                (
                    "Removed {Count} old unauthenticated users from the database.",
                    count
                );
        }

        return Result.FromSuccess();
    }
}
