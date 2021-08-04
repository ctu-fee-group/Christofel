using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Christofel.CommandsLib.CommandsInfo;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace Christofel.CommandsLib.HandlerCreator
{
    /// <summary>
    /// Creates SlashCommandHandler for command with one or more level subcommands.
    /// Matchers should match against name of all of the subcommands
    ///
    /// So for example, if we have /command subcmd subsubcmd, then
    /// matcher for "subcmd subsubcmd" should be present
    /// </summary>
    public class SubCommandHandlerCreator : ICommandHandlerCreator<string, Delegate>
    {
        private record HandlerMatcher(Func<string, bool> Matcher,
            Func<SocketSlashCommand, SocketSlashCommandDataOption?, CancellationToken, Task> Handler);

        public SlashCommandHandler CreateHandlerForCommand(IEnumerable<(Func<string, bool>, Delegate)> matchers)
        {
            List<HandlerMatcher> realMatchers = matchers
                .Select(x => new HandlerMatcher(
                    x.Item1,
                    CommandHandlerCreatorUtils.CreateHandler<SocketSlashCommandDataOption?>(x.Item2,
                        (data, option) => CommandHandlerCreatorUtils.GetParametersFromOptions(x.Item2, option?.Options))
                ))
                .ToList();

            return GetHandler(realMatchers);
        }

        private SlashCommandHandler GetHandler(List<HandlerMatcher> matchers)
        {
            return (command, token) =>
            {
                token.ThrowIfCancellationRequested();

                string matchAgainst = GetSubcommandArguments(command, out SocketSlashCommandDataOption? option);
                HandlerMatcher? handler = matchers.FirstOrDefault(x => x.Matcher(matchAgainst));

                if (handler is null)
                {
                    throw new InvalidOperationException(
                        $"Could not match subcommand /{command.Data.Name} {matchAgainst}");
                }

                return handler.Handler(command, option, token);
            };
        }

        private string GetSubcommandArguments(SocketSlashCommand command,
            out SocketSlashCommandDataOption? currentOption)
        {
            List<string> subcommands = new();
            SocketSlashCommandDataOption? nextOption = command.Data.Options.FirstOrDefault();
            currentOption = null;

            IReadOnlyCollection<SocketSlashCommandDataOption>? options = command.Data.Options;

            while (nextOption?.Type is (ApplicationCommandOptionType.SubCommandGroup or ApplicationCommandOptionType
                .SubCommand))
            {
                subcommands.Add(nextOption.Name);

                currentOption = nextOption;
                options = options?.First().Options;
                nextOption = options?.FirstOrDefault();
            }

            return string.Join(' ', subcommands);
        }
    }
}