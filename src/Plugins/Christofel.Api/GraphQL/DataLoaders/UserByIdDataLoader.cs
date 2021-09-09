//
//   UserByIdDataLoader.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Implementations.ReadOnlyDatabase;
using GreenDonut;
using HotChocolate.DataLoader;
using Microsoft.EntityFrameworkCore;

namespace Christofel.Api.GraphQL.DataLoaders
{
    /// <summary>
    ///     Loads DbUser from database by user id
    /// </summary>
    public class UserByIdDataLoader : BatchDataLoader<int, DbUser>
    {
        private readonly ReadonlyDbContextFactory<ChristofelBaseContext> _dbContextFactory;

        public UserByIdDataLoader
        (
            IBatchScheduler batchScheduler,
            ReadonlyDbContextFactory<ChristofelBaseContext> dbContextFactory,
            DataLoaderOptions<int>? options = null
        )
            : base(batchScheduler, options)
        {
            _dbContextFactory = dbContextFactory;
        }

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