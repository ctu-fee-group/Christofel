//
//   CoursesContextDesignFactory.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Application;
using Christofel.BaseLib.Extensions;
using Christofel.Common.Database;
using Christofel.CoursesLib.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.Design;

/// <summary>
/// Factory of <see cref="CoursesContext"/>.
/// </summary>
public class CoursesContextDesignFactory : IDesignTimeDbContextFactory<CoursesContext>
{
    /// <inheritdoc />
    public CoursesContext CreateDbContext(string[] args)
    {
        var services = new ServiceCollection()
            .AddChristofelDbContextFactory<CoursesContext>(ChristofelApp.CreateConfiguration(args))
            .BuildServiceProvider();

        return services.GetRequiredService<IDbContextFactory<CoursesContext>>().CreateDbContext();
    }
}