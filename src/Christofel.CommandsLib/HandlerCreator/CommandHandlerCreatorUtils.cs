using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Christofel.CommandsLib.CommandsInfo;
using Christofel.CommandsLib.Reflection;
using Discord.WebSocket;

namespace Christofel.CommandsLib.HandlerCreator
{
    public class CommandHandlerCreatorUtils
    {
        /// <summary>
        /// Create SlashCommandHandler from given Delegate.
        /// Uses <see cref="EfficientInvoker"/> for invoking the delegate faster.
        /// </summary>
        /// <param name="function"></param>
        /// <param name="getArguments"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static SlashCommandHandler CreateHandler(Delegate function,
            Func<SocketSlashCommandData, IEnumerable<object?>?> getArguments)
        {
            EfficientInvoker invoker = EfficientInvoker.ForDelegate(function);

            return (command, token) =>
            {
                List<object?> args = new() {command};
                args.AddRange(getArguments(command.Data) ?? Enumerable.Empty<object?>());
                args.Add(token);

                return Invoke(command.Data.Name, invoker, function, args.ToArray());
            };
        }

        /// <summary>
        /// Create SlashCommandHandler from given Delegate.
        /// Uses <see cref="EfficientInvoker"/> for invoking the delegate faster.
        /// </summary>
        /// <param name="function"></param>
        /// <param name="getArguments"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static Func<SocketSlashCommand, T, CancellationToken, Task> CreateHandler<T>(Delegate function,
            Func<SocketSlashCommandData, T, IEnumerable<object?>?> getArguments)
        {
            EfficientInvoker invoker = EfficientInvoker.ForDelegate(function);

            return (command, helper, token) =>
            {
                List<object?> args = new() {command};
                args.AddRange(getArguments(command.Data, helper) ?? Enumerable.Empty<object?>());
                args.Add(token);

                return Invoke(command.Data.Name, invoker, function, args.ToArray());
            };
        }

        public static IEnumerable<object?> GetParametersFromOptions(Delegate function,
            IEnumerable<SocketSlashCommandDataOption>? options)
        {
            ParameterInfo[] parameters = function.Method.GetParameters();
            object?[] arguments = new object?[parameters.Length - 2];

            if (options == null)
            {
                return arguments;
            }

            IEnumerable<SocketSlashCommandDataOption> socketSlashCommandDataOptions =
                options as SocketSlashCommandDataOption[] ?? options.ToArray();
            for (int i = 0; i < arguments.Length; i++)
            {
                string name = parameters[i + 1].Name?.ToLower() ?? "";
                arguments[i] = socketSlashCommandDataOptions.FirstOrDefault(x => x.Name == name)?.Value;
            }

            return arguments;
        }

        private static Task Invoke(string name, EfficientInvoker invoker, Delegate function, object?[] args)
        {
            object? data = invoker.Invoke(function, args.ToArray());

            if (data is null)
            {
                throw new InvalidOperationException($@"Command handler of {name} returned null instead of Task");
            }

            return (Task) data;
        }
    }
}