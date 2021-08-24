using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Christofel.BaseLib.Extensions
{
    public static class IServiceCollectionExtensions
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
                .AddSingleton(state.Bot.Client)
                .AddSingleton(state.Bot.Cache)
                .AddSingleton(state.Bot.HttpClientFactory)
                .AddSingleton(state.Lifetime)
                .AddSingleton(state.LoggerFactory)
                .AddSingleton(typeof(ILogger<>), typeof(Logger<>))
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