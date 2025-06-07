//
//   ReactHandlerContextDesignFactory.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Application;
using Christofel.BaseLib.Extensions;
using Christofel.ReactHandler.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

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
            var services = new ServiceCollection()
                .AddChristofelDbContextFactory<ReactHandlerContext>(ChristofelApp.CreateConfiguration(args))
                .BuildServiceProvider();

            return services.GetRequiredService<IDbContextFactory<ReactHandlerContext>>().CreateDbContext();
        }
    }
}