//
//   ServiceCollectionExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.BaseLib.Extensions;
using Christofel.CoursesLib.Database;
using Christofel.CoursesLib.Services;
using Christofel.Helpers.ReadOnlyDatabase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.CoursesLib.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Christofel courses services and databases.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="configuration">The configuration for database key.</param>
    /// <returns>The service collection returned.</returns>
    public static IServiceCollection AddCourses(this IServiceCollection serviceCollection, IConfiguration configuration)
        => serviceCollection
            .AddMemoryCache()
            .AddChristofelDbContextFactory<CoursesContext>(configuration)
            .AddReadOnlyDbContext<CoursesContext>()
            .AddTransient<CoursesChannelUserAssigner>()
            .AddTransient<CurrentSemesterCache>()
            .AddTransient<CoursesChannelCreator>()
            .AddTransient<CoursesRepository>();
}