//
//   RemoveOldUsersJob.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Database;
using Christofel.Scheduling;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.Management.Jobs
{
    /// <summary>
    /// Job for removing users that are not authenticated and were created before 2 days or more.
    /// </summary>
    public class RemoveOldUsersJob : IJob
    {
        private readonly ChristofelBaseContext _dbContext;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveOldUsersJob"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="logger">The logger.</param>
        public RemoveOldUsersJob(ChristofelBaseContext dbContext, ILogger<RemoveOldUsersJob> logger)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<Result> ExecuteAsync(IJobContext jobContext, CancellationToken ct = default)
        {
            var beforeTwoDays = DateTime.Now.Subtract(TimeSpan.FromDays(2));
            var count = 0;

            foreach (var userToDelete in _dbContext.Users
                .Where(x => x.AuthenticatedAt == null && x.CreatedAt < beforeTwoDays))
            {
                _dbContext.Remove(userToDelete);
                count++;
            }

            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Removed {Count} old unauthenticated users from the database", count);

            return Result.FromSuccess();
        }
    }
}