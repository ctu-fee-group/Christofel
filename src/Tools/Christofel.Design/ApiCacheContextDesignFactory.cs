//
//   ApiCacheContextDesignFactory.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Api.Ctu.Database;
using Christofel.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Christofel.Design
{
    public class ApiCacheContextDesignFactory : IDesignTimeDbContextFactory<ApiCacheContext>
    {
        public ApiCacheContext CreateDbContext(string[] args)
        {
            DbContextOptionsBuilder<ApiCacheContext> builder = new DbContextOptionsBuilder<ApiCacheContext>();
            IConfiguration configuration = ChristofelApp.CreateConfiguration(args);
            string connectionString = configuration.GetConnectionString("ApiCache");
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new ApiCacheContext(builder.Options);
        }
    }
}