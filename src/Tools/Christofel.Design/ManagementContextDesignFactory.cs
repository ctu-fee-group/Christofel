//
//   ManagementContextDesignFactory.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Application;
using Christofel.BaseLib.Extensions;
using Christofel.Management.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.Design
{
    /// <summary>
    /// Factory of <see cref="ManagementContext"/>.
    /// </summary>
    public class ManagementContextDesignFactory : IDesignTimeDbContextFactory<ManagementContext>
    {
        /// <inheritdoc />
        public ManagementContext CreateDbContext(string[] args)
        {
            var services = new ServiceCollection()
                .AddChristofelDbContextFactory<ManagementContext>(ChristofelApp.CreateConfiguration(args))
                .BuildServiceProvider();

            return services.GetRequiredService<IDbContextFactory<ManagementContext>>().CreateDbContext();
        }
    }
}