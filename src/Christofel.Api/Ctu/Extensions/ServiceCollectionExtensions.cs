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
            collection.AddOptions<TypeRepository<IPreAuthCondition>>();
            collection.AddOptions<TypeRepository<IAuthStep>>();
            collection.AddOptions<TypeRepository<IAuthTask>>();
            
            collection
                .AddScoped<CtuAuthProcess>();

            return collection;
        }
        
        public static IServiceCollection AddAuthStep<T>(this IServiceCollection collection)
            where T : class, IAuthStep
        {
            collection
                .TryAddTransient<T>();

            collection.Configure<TypeRepository<IAuthStep>>(
                provider => provider.RegisterType<T>()
            );

            return collection;
        }
        
        public static IServiceCollection AddAuthTask<T>(this IServiceCollection collection)
            where T : class, IAuthTask
        {
            collection
                .TryAddTransient<T>();

            collection.Configure<TypeRepository<IAuthTask>>(
                provider => provider.RegisterType<T>()
            );

            return collection;
        }
        
        public static IServiceCollection AddAuthCondition<T>(this IServiceCollection collection)
            where T : class, IPreAuthCondition
        {
            collection
                .TryAddTransient<T>();

            collection.Configure<TypeRepository<IPreAuthCondition>>(
                provider => provider.RegisterType<T>()
            );

            return collection;
        }
    }
}