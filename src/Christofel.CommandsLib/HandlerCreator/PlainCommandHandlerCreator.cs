using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Christofel.CommandsLib.CommandsInfo;
using Discord;

namespace Christofel.CommandsLib.HandlerCreator
{
    /// <summary>
    /// Creates SlashCommandHandler for command without subcommands.
    /// It should receive only 1 matcher and that should always match
    /// </summary>
    public class PlainCommandHandlerCreator : ICommandHandlerCreator<string>
    {
        public SlashCommandHandler CreateHandlerForCommand(IEnumerable<(Func<string, bool>, Delegate)> matchers)
        {
            var valueTuples = matchers as (Func<string, bool>, Delegate)[] ?? matchers.ToArray();
            if (valueTuples.Count() != 1)
            {
                throw new InvalidOperationException("PlainCommandHandlerCreator can handle only one matcher that is always true");
            }
            
            var matcher = valueTuples.FirstOrDefault();
            return GetHandler(CommandHandlerCreatorUtils.CreateHandler(matcher.Item2, (data => data.Options.Select(x => x.Value))));
        }

        private SlashCommandHandler GetHandler(SlashCommandHandler handler)
        {
            return (command, token) =>
            {
                token.ThrowIfCancellationRequested();
                return handler(command, token);
            };
        }
    }
}