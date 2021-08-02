using System;
using System.Linq;
using Christofel.CommandsLib.CommandsInfo;
using Christofel.CommandsLib.HandlerCreator;

namespace Christofel.CommandsLib.Extensions
{
    public static class ICommandHandlerCreatorExtensions
    {
        /// <summary>
        /// Passes one matcher that always return true to <see cref="ICommandHandlerCreator{T}.CreateHandlerForCommand"/>
        /// Creates SlashCommandHandler that will always execute given delegate
        /// </summary>
        /// <param name="creator"></param>
        /// <param name="deleg">Delegate to execute with the command</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static SlashCommandHandler CreateHandlerForCommand<T>(this ICommandHandlerCreator<T, Delegate> creator,
            Delegate deleg)
        {
            return creator
                .CreateHandlerForCommand((_ => true, deleg));
        }

        /// <summary>
        /// Passes params to <see cref="ICommandHandlerCreator{T}.CreateHandlerForCommand"/>
        /// Creates SlashCommandHandler based on matchers
        /// </summary>
        /// <param name="creator"></param>
        /// <param name="matchers"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static SlashCommandHandler CreateHandlerForCommand<T, U>(this ICommandHandlerCreator<T, U> creator, params (Func<T, bool>, U)[] matchers)
        {
            return creator.CreateHandlerForCommand(matchers.AsEnumerable());
        }

        /// <summary>
        /// Passes equals matchers to <see cref="ICommandHandlerCreator{T}.CreateHandlerForCommand"/>
        /// Creates SlashCommandHandler matching T objects in matchers
        /// </summary>
        /// <param name="creator"></param>
        /// <param name="matchers"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static SlashCommandHandler CreateHandlerForCommand<T, U>(this ICommandHandlerCreator<T, U> creator,
            params (T, U)[] matchers)
        where T : notnull
        {
            return creator
                .CreateHandlerForCommand(matchers.Select(
                    x => ValueTuple.Create<Func<T, bool>, U>(((y) => x.Item1.Equals(y)), x.Item2)));
        }
    }
}