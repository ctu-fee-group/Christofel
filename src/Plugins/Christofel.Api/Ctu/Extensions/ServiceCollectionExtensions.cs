//
//   ServiceCollectionExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Api.Ctu.Auth.Conditions;
using Christofel.Api.Ctu.Auth.Steps;
using Christofel.Api.Ctu.Auth.Tasks;
using Christofel.Api.Ctu.Resolvers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Christofel.Api.Ctu.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCtuAuthProcess(this IServiceCollection collection)
        {
            collection
                .TryAddScoped<CtuAuthProcess>();

            collection
                .TryAddScoped<ICtuTokenProvider, CtuTokenProvider>();

            return collection;
        }

        public static IServiceCollection AddDefaultCtuAuthProcess(this IServiceCollection services)
        {
            services
                .AddScoped<DuplicateResolver>()
                .AddScoped<NicknameResolver>()
                .AddAuthCondition<CtuUsernameFilledCondition>()
                .AddAuthCondition<MemberMatchesUserCondition>()
                .AddAuthCondition<NoDuplicateCondition>()
                .AddAuthCondition<CtuUsernameMatchesCondition>()
                .AddAuthStep<ProgrammeRoleStep>()
                .AddAuthStep<SetUserDataStep>()
                .AddAuthStep<SpecificRolesStep>()
                .AddAuthStep<TitlesRoleStep>()
                .AddAuthStep<UsermapRolesStep>()
                .AddAuthStep<YearRoleStep>()
                .AddAuthStep<DuplicateAssignStep>()
                .AddAuthStep<RemoveOldRolesStep>()
                .AddAuthTask<AssignRolesAuthTask>()
                .AddAuthTask<SetNicknameAuthTask>();

            return services;
        }

        public static IServiceCollection AddAuthStep<T>(this IServiceCollection collection)
            where T : class, IAuthStep
        {
            collection
                .AddTransient<IAuthStep, T>();

            return collection;
        }

        public static IServiceCollection AddAuthTask<T>(this IServiceCollection collection)
            where T : class, IAuthTask
        {
            collection
                .AddTransient<IAuthTask, T>();

            return collection;
        }

        public static IServiceCollection AddAuthCondition<T>(this IServiceCollection collection)
            where T : class, IPreAuthCondition
        {
            collection
                .AddTransient<IPreAuthCondition, T>();

            return collection;
        }
    }
}