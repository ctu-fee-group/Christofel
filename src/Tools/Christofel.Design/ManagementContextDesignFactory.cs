//
//   ManagementContextDesignFactory.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Application;
using Christofel.Management.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Christofel.Design
{
    public class ManagementContextDesignFactory : IDesignTimeDbContextFactory<ManagementContext>
    {
        public ManagementContext CreateDbContext(string[] args)
        {
            DbContextOptionsBuilder<ManagementContext> builder = new DbContextOptionsBuilder<ManagementContext>();
            IConfiguration configuration = ChristofelApp.CreateConfiguration(args);
            string connectionString = configuration.GetConnectionString("Management");
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new ManagementContext(builder.Options);
        }
    }
}