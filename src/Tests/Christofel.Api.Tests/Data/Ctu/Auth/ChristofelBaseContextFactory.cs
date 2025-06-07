//
//   ChristofelBaseContextFactory.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Common.Database;
using Microsoft.EntityFrameworkCore;
using TestSupport.EfHelpers;

namespace Christofel.Api.Tests.Data.Ctu.Auth
{
    /// <summary>
    /// Dummy factory of <see cref="ChristofelBaseContext"/> that will create in-memory database.
    /// </summary>
    public class ChristofelBaseContextFactory : IDbContextFactory<ChristofelBaseContext>
    {
        private readonly DbContextOptionsDisposable<ChristofelBaseContext> _dbContextOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChristofelBaseContextFactory"/> class.
        /// </summary>
        /// <param name="dbContextOptions">The options to create the context with.</param>
        public ChristofelBaseContextFactory(DbContextOptionsDisposable<ChristofelBaseContext> dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;
        }

        /// <inheritdoc />
        public ChristofelBaseContext CreateDbContext() => new ChristofelBaseContext(_dbContextOptions);
    }
}