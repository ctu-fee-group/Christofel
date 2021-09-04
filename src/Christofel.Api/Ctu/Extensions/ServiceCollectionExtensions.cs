using Christofel.Api.Ctu.Auth.Conditions;
using Christofel.Api.Ctu.Auth.Steps;
using Christofel.Api.Ctu.Auth.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Remora.Commands.Services;

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
                .AddAuthStep<SetNicknameAuthStep>()
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
                .TryAddTransient<IAuthStep, T>();

            return collection;
        }
        
        public static IServiceCollection AddAuthTask<T>(this IServiceCollection collection)
            where T : class, IAuthTask
        {
            collection
                .TryAddTransient<IAuthTask, T>();

            return collection;
        }
        
        public static IServiceCollection AddAuthCondition<T>(this IServiceCollection collection)
            where T : class, IPreAuthCondition
        {
            collection
                .TryAddTransient<IPreAuthCondition, T>();
            
            return collection;
        }
    }
}