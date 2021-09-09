//
//   ChristofelBaseContextFactory.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.BaseLib.Database;
using Microsoft.EntityFrameworkCore;
using TestSupport.EfHelpers;

namespace Christofel.Api.Tests.Data.Ctu.Auth
{
    public class ChristofelBaseContextFactory : IDbContextFactory<ChristofelBaseContext>
    {
        private readonly DbContextOptionsDisposable<ChristofelBaseContext> _dbContextOptions;

        public ChristofelBaseContextFactory(DbContextOptionsDisposable<ChristofelBaseContext> dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;
        }

        public ChristofelBaseContext CreateDbContext() => new ChristofelBaseContext(_dbContextOptions);
    }
}