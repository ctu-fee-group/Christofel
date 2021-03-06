//
//   ApplicationCommandExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using Remora.Discord.API.Abstractions.Objects;

namespace Christofel.CommandsLib.Extensions
{
    /// <summary>
    /// Class containing extensions for <see cref="IApplicationCommand"/>.
    /// </summary>
    public static class ApplicationCommandExtensions
    {
        /// <summary>
        /// Checks if the <paramref name="command"/> matches <paramref name="commandData"/>.
        /// </summary>
        /// <param name="command">The command to match against <paramref name="commandData"/>.</param>
        /// <param name="commandData">The bulk data to match against <paramref name="command"/>.</param>
        /// <returns>Whether <paramref name="command"/> matches <paramref name="commandData"/>.</returns>
        public static bool MatchesBulkCommand
        (
            this IApplicationCommand command,
            IBulkApplicationCommandData commandData
        )
        {
            if (command.Name != commandData.Name ||
                (command.Description != commandData.Description && command.Type != commandData.Type) ||
                !commandData.Options.HasSameLength(command.Options))
            {
                return false;
            }

            return !command.Options.HasValue || !commandData.Options.HasValue ||
                   command.Options.Value.OrderBy(x => x.Name).ToList()
                       .CollectionMatches
                       (
                           commandData.Options.Value.OrderBy(x => x.Name).ToList(),
                           CommandOptionMatches
                       );
        }

        private static bool CommandOptionMatches
        (
            IApplicationCommandOption left,
            IApplicationCommandOption right
        )
        {
            if (left.Name != right.Name || left.Description != right.Description || left.Type != right.Type ||
                !left.IsDefault.CheckOptionalBoolMatches(right.IsDefault, false) ||
                !left.IsRequired.CheckOptionalBoolMatches(right.IsRequired, false) ||
                !left.Options.HasSameLength(right.Options) || !left.Choices.HasSameLength(right.Choices))
            {
                return false;
            }

            if (left.Options.HasValue && right.Options.HasValue &&
                !left.Options.Value.OrderBy(x => x.Name).ToList()
                    .CollectionMatches(right.Options.Value.OrderBy(x => x.Name).ToList(), CommandOptionMatches))
            {
                return false;
            }

            return !left.Choices.HasValue || !right.Choices.HasValue ||
                   left.Choices.Value.CollectionMatches(right.Choices.Value, CommandChoiceMatches);
        }

        private static bool CommandChoiceMatches
        (
            IApplicationCommandOptionChoice leftChoice,
            IApplicationCommandOptionChoice rightChoice
        )
            => leftChoice.Value.Equals(rightChoice.Value) && leftChoice.Name == rightChoice.Name;
    }
}