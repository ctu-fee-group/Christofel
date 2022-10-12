//
//  ServiceCollectionExtensions.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Helpers.Localization.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.Helpers.Localization;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds localization using json.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddJsonLocalization(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>()
            .AddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>))
            .AddTransient(typeof(LocalizedStringLocalizer<>))
            .AddTransient(p => p.GetRequiredService<IStringLocalizerFactory>().Create("Default"));
    }
}