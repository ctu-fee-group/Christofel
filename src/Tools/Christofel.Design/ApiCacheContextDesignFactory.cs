//
//   ApiCacheContextDesignFactory.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Christofel.Api.Ctu.Database;
using Christofel.Application;
using Christofel.BaseLib.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.Design
{
    /// <summary>
    /// Factory of <see cref="ApiCacheContext"/>.
    /// </summary>
    public class ApiCacheContextDesignFactory : IDesignTimeDbContextFactory<ApiCacheContext>
    {
        /// <inheritdoc />
        public ApiCacheContext CreateDbContext(string[] args)
        {
            var services = new ServiceCollection()
                .AddChristofelDbContextFactory<ApiCacheContext>(ChristofelApp.CreateConfiguration(args))
                .BuildServiceProvider();

            return services.GetRequiredService<IDbContextFactory<ApiCacheContext>>().CreateDbContext();
        }
    }
}