//
//   UserByIdDataLoader.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Common.Database;
using Christofel.Common.Database.Models;
using Christofel.Helpers.ReadOnlyDatabase;
using GreenDonut;
using Microsoft.EntityFrameworkCore;

namespace Christofel.Api.GraphQL.DataLoaders
{
    /// <summary>
    /// Loads DbUser from database by user id.
    /// </summary>
    public class UserByIdDataLoader : BatchDataLoader<int, DbUser>
    {
        private readonly ReadonlyDbContextFactory<ChristofelBaseContext> _dbContextFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserByIdDataLoader"/> class.
        /// </summary>
        /// <param name="batchScheduler">The batch scheduler.</param>
        /// <param name="dbContextFactory">The christofel base context factory.</param>
        /// <param name="options">The options of the data loader.</param>
        public UserByIdDataLoader
        (
            IBatchScheduler batchScheduler,
            ReadonlyDbContextFactory<ChristofelBaseContext> dbContextFactory,
            DataLoaderOptions? options = null
        )
            : base(batchScheduler, options)
        {
            _dbContextFactory = dbContextFactory;
        }

        /// <inheritdoc />
        protected override async Task<IReadOnlyDictionary<int, DbUser>> LoadBatchAsync
            (IReadOnlyList<int> keys, CancellationToken cancellationToken)
        {
            await using IReadableDbContext dbContext = _dbContextFactory.CreateDbContext();

            return await dbContext
                .Set<DbUser>()
                .Where(x => keys.Contains(x.UserId))
                .ToDictionaryAsync(x => x.UserId, cancellationToken);
        }
    }
}