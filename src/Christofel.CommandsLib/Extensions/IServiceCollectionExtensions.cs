using System;
using System.Runtime.CompilerServices;
using Christofel.CommandsLib.Commands;
using Christofel.CommandsLib.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Christofel.CommandsLib.Extensions
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds default InteractionHandler to the service collection
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="configure">Function that is to make code more readable, it is invoken with the same collection that was passed to the function. It should register CommandGroups</param>
        /// <returns></returns>
        public static IServiceCollection AddDefaultInteractionHandler(this IServiceCollection collection, Action<IServiceCollection>? configure = null)
        {
            collection
                .AddOptions<CommandGroupsService>();

            collection
                .AddSingleton<ICommandHolder, CommandHolder>()
                .AddSingleton<InteractionHandler>();

            configure?.Invoke(collection);
            
            return collection;
        }
        
        public static IServiceCollection AddCommandGroup<T>(this IServiceCollection collection)
            where T : class, ICommandGroup
        {
            collection.AddSingleton<T>();

            collection.Configure<CommandGroupsService>(handler =>
                handler.RegisterGroupType(typeof(T)));

            return collection;
        }
    }
}