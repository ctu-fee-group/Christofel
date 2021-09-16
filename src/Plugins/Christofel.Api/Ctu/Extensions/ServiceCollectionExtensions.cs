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
    /// <summary>
    /// Class containing extensions for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds ctu auth process class.
        /// </summary>
        /// <param name="collection">The collection to add to.</param>
        /// <returns>The passed collection.</returns>
        public static IServiceCollection AddCtuAuthProcess(this IServiceCollection collection)
        {
            collection
                .TryAddScoped<CtuAuthProcess>();

            collection
                .TryAddScoped<ICtuTokenProvider, CtuTokenProvider>();

            return collection;
        }

        /// <summary>
        /// Adds all default ctu auth conditions, steps and tasks.
        /// </summary>
        /// <param name="services">The collection to add to.</param>
        /// <returns>The passed collection.</returns>
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
                .AddAuthTask<SetNicknameAuthTask>()
                .AddAuthTask<SendNoRolesMessageAuthTask>();

            return services;
        }

        /// <summary>
        /// Adds <see cref="IAuthStep"/> to the collection.
        /// </summary>
        /// <param name="collection">The collection to add to.</param>
        /// <typeparam name="T">The type of the auth step.</typeparam>
        /// <returns>The passed collection.</returns>
        public static IServiceCollection AddAuthStep<T>(this IServiceCollection collection)
            where T : class, IAuthStep
        {
            collection
                .AddTransient<IAuthStep, T>();

            return collection;
        }

        /// <summary>
        /// Adds <see cref="IAuthTask"/> to the collection.
        /// </summary>
        /// <param name="collection">The collection to add to.</param>
        /// <typeparam name="T">The type of the auth task.</typeparam>
        /// <returns>The passed collection.</returns>
        public static IServiceCollection AddAuthTask<T>(this IServiceCollection collection)
            where T : class, IAuthTask
        {
            collection
                .AddTransient<IAuthTask, T>();

            return collection;
        }

        /// <summary>
        /// Adds <see cref="IPreAuthCondition"/> to the collection.
        /// </summary>
        /// <param name="collection">The collection to add to.</param>
        /// <typeparam name="T">The type of the auth condition.</typeparam>
        /// <returns>The passed collection.</returns>
        public static IServiceCollection AddAuthCondition<T>(this IServiceCollection collection)
            where T : class, IPreAuthCondition
        {
            collection
                .AddTransient<IPreAuthCondition, T>();

            return collection;
        }
    }
}