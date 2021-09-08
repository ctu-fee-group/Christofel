using System.Linq;
using Remora.Discord.API.Abstractions.Objects;

namespace Christofel.CommandsLib.Extensions
{
    public static class ApplicationCommandExtensions
    {
        public static bool MatchesBulkCommand(this IApplicationCommand command, bool defaultPermission,
            IBulkApplicationCommandData commandData)
        {
            if (command.Name != commandData.Name ||
                command.Description != commandData.Description && command.Type != commandData.Type ||
                ((command.DefaultPermission.HasValue ? command.DefaultPermission : false) != defaultPermission) ||
                !commandData.Options.HasSameLength(command.Options))
            {
                return false;
            }

            return !command.Options.HasValue || !commandData.Options.HasValue ||
                   command.Options.Value.OrderBy(x => x.Name).ToList()
                       .CollectionMatches(commandData.Options.Value.OrderBy(x => x.Name).ToList(),
                           CommandOptionMatches);
        }


        private static bool CommandOptionMatches(IApplicationCommandOption left,
            IApplicationCommandOption right)
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

        private static bool CommandChoiceMatches(IApplicationCommandOptionChoice leftChoice,
            IApplicationCommandOptionChoice rightChoice)
        {
            return (leftChoice.Value.Equals(rightChoice.Value) && leftChoice.Name == rightChoice.Name);
        }
    }
}