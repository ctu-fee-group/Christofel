using Christofel.Plugins.Runtime;
using Christofel.Plugins.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.Plugins
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPlugins(this IServiceCollection services)
        {
            services
                .AddOptions<PluginServiceOptions>();

            return services
                .AddSingleton<PluginStorage>()
                .AddSingleton<PluginAssemblyService>()
                .AddSingleton<PluginLifetimeService>()
                .AddSingleton<PluginService>();
        }

        public static IServiceCollection AddRuntimePlugins<TState, TContext>(this IServiceCollection services)
        {
            return services
                .AddSingleton<RuntimePluginService<TState, TContext>>()
                .AddSingleton<IPluginLifetimeService, RuntimePluginService<TState, TContext>>(p =>
                    p.GetRequiredService<RuntimePluginService<TState, TContext>>());
        }
    }
}