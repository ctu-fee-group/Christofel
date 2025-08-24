//
//   ServiceCollectionExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.CtuAuth.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.CtuAuth.Tests.Data.Ctu.Auth;

/// <summary>
/// An extension clas for IServiceCollection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add successful condition, step and task.
    /// </summary>
    /// <param name="services">The service collection to extend with successful condition, step and task.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddCtuSuccessfulCST(this IServiceCollection services)
    {
        return services
            .AddAuthCondition<ConditionRepository.SuccessfulCondition>()
            .AddAuthStep<StepRepository.SuccessfulStep>()
            .AddAuthTask<TaskRepository.SuccessfulTask>();
    }
}
