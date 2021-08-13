using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Christofel.Api.Ctu.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCtuAuthProcess(this IServiceCollection collection)
        {
            collection
                .AddScoped<CtuAuthProcess>()
                .AddTransient<CtuAuthStepProvider>(p =>
                {
                    CtuAuthStepProvider stepProvider = p.GetRequiredService<IOptions<CtuAuthStepProvider>>().Value;
                    stepProvider.Provider = p;

                    return stepProvider;
                });

            collection
                .AddOptions<CtuAuthStepProvider>();
            
            return collection;
        }
        
        public static IServiceCollection AddCtuAuthStep<T>(this IServiceCollection collection)
            where T : class, ICtuAuthStep
        {
            collection
                .TryAddTransient<T>();

            collection.Configure<CtuAuthStepProvider>(
                provider => provider.AddStep(typeof(T))
            );

            return collection;
        }
    }
}