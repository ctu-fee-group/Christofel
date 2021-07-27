using System;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.BaseLib.Extensions
{
    public static class IServiceProviderExtensions
    {
        /// <summary>
        /// Adds Christofel state and it's properties to provider
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IServiceCollection AddDiscordState(this IServiceCollection provider, IChristofelState state)
        {
            return provider
                .AddSingleton(state)
                .AddSingleton(state.Permissions.Resolver)
                .AddSingleton(state.Configuration)
                .AddSingleton(state.Bot)
                .AddSingleton(state.Permissions);
        }

        /// <summary>
        /// Adds Christofel database context factory and read only database factory
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IServiceCollection AddChristofelDatabase(this IServiceCollection provider, IChristofelState state, bool write = true)
        {

            if (write)
            {
                provider.AddSingleton(state.DatabaseFactory);
            }
            
            return provider
                .AddSingleton(state.ReadOnlyDatabaseFactory);
        }
    }
}