using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        public static SlashCommandHandler CreateHandler(Delegate function, Func<SocketSlashCommandData, IEnumerable<object?>?> getArguments)
        {
            EfficientInvoker invoker = EfficientInvoker.ForDelegate(function);
            
            return (command, token) =>
            {
                List<object?> args = new() {command};
                args.AddRange(getArguments(command.Data) ?? Enumerable.Empty<object?>());
                args.Add(token);

                object? data = invoker.Invoke(function, args.ToArray());

                if (data is null)
                {
                    throw new InvalidOperationException($@"Command handler of {command.Data.Name} returned null instead of Task");
                }

                return (Task)data;
            };
        }
    }
}