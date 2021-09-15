//
//   ChristofelBaseContextDesignFactory.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Api.Ctu.Database;
using Christofel.Application;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.Design
{
    /// <summary>
    /// Factory of <see cref="ChristofelBaseContext"/>.
    /// </summary>
    public class ChristofelBaseContextDesignFactory : IDesignTimeDbContextFactory<ChristofelBaseContext>
    {
        /// <inheritdoc />
        public ChristofelBaseContext CreateDbContext(string[] args)
        {
            var services = new ServiceCollection()
                .AddChristofelDbContextFactory<ChristofelBaseContext>(ChristofelApp.CreateConfiguration(args))
                .BuildServiceProvider();

            return services.GetRequiredService<IDbContextFactory<ChristofelBaseContext>>().CreateDbContext();
        }
    }
}