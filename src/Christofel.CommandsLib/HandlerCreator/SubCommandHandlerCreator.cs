using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Christofel.CommandsLib.CommandsInfo;
using Discord;
using Microsoft.EntityFrameworkCore;

namespace Christofel.CommandsLib.HandlerCreator
{
    /// <summary>
    /// Creates SlashCommandHandler for command with one level subcommands.
    /// Matchers should match against name of the subcommand
    /// </summary>
    public class SubCommandHandlerCreator : ICommandHandlerCreator<string>
    {
        private record HandlerMatcher(Func<string, bool> Matcher, SlashCommandHandler Handler);
        
        public SlashCommandHandler CreateHandlerForCommand(IEnumerable<(Func<string, bool>, Delegate)> matchers)
        {
            List<HandlerMatcher> realMatchers = matchers
                .Select(x => new HandlerMatcher(
                    x.Item1,
                    CommandHandlerCreatorUtils.CreateHandler(x.Item2,
                        data => CommandHandlerCreatorUtils.GetParametersFromOptions(x.Item2, data.Options.First().Options))
                ))
                .ToList();

            return GetHandler(realMatchers);
        }

        private SlashCommandHandler GetHandler(List<HandlerMatcher> matchers)
        {
            return (command, token) =>
            {
                token.ThrowIfCancellationRequested();
                string matchAgainst = command.Data.Options.First().Name;
                HandlerMatcher? handler = matchers.FirstOrDefault(x => x.Matcher(matchAgainst));

                if (handler is null)
                {
                    throw new InvalidOperationException(
                        $"Could not match subcommand /{command.Data.Name} {matchAgainst}");
                }

                return handler.Handler(command, token);
            };
        }
    }
}