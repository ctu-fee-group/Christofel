//
//   ReactHandlerContextDesignFactory.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Application;
using Christofel.ReactHandler.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Christofel.Design
{
    /// <summary>
    /// Factory of <see cref="ReactHandlerContext"/>.
    /// </summary>
    public class ReactHandlerContextDesignFactory : IDesignTimeDbContextFactory<ReactHandlerContext>
    {
        /// <inheritdoc />
        public ReactHandlerContext CreateDbContext(string[] args)
        {
            DbContextOptionsBuilder<ReactHandlerContext> builder = new DbContextOptionsBuilder<ReactHandlerContext>();
            IConfiguration configuration = ChristofelApp.CreateConfiguration(args);
            string connectionString = configuration.GetConnectionString("ReactHandler");
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new ReactHandlerContext(builder.Options);
        }
    }
}