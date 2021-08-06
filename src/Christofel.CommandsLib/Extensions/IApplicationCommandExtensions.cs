using System.Collections.Generic;
using System.Linq;
using System.Security;
using Discord;

namespace Christofel.CommandsLib.Extensions
{
    public static class IApplicationCommandExtensions
    {
        public static bool MatchesCreationProperties(this IApplicationCommand command,
            SlashCommandCreationProperties creationProperties)
        {
            if (command.DefaultPermission != creationProperties.DefaultPermission.GetValueOrDefault(false) ||
                command.Description != creationProperties.Description || command.Name != creationProperties.Name)
            {
                return false;
            }


            List<ApplicationCommandOptionProperties> creationOptions =
                creationProperties.Options.GetValueOrDefault(new List<ApplicationCommandOptionProperties>());
            List<IApplicationCommandOption> commandOptions = command.Options?.ToList() ?? new List<IApplicationCommandOption>();
            return MatchesCreationOptions(creationOptions, commandOptions);
        }

        private static bool MatchesCreationOption(this IApplicationCommandOption commandOption,
            ApplicationCommandOptionProperties creationOption)
        {
            if (commandOption.Default != creationOption.Default ||
                commandOption.Description != creationOption.Description || commandOption.Name != creationOption.Name ||
                (commandOption.Required ?? false) != creationOption.Required || commandOption.Type != creationOption.Type)
            {
                return false;
            }

            if (!MatchesChoices(commandOption, creationOption))
            {
                return false;
            }
            
            List<ApplicationCommandOptionProperties> creationOptions =
                creationOption.Options ?? new List<ApplicationCommandOptionProperties>();
            List<IApplicationCommandOption> commandOptions = commandOption.Options?.ToList() ?? new List<IApplicationCommandOption>();
            
            return MatchesCreationOptions(creationOptions, commandOptions);
        }

        private static bool MatchesChoices(IApplicationCommandOption commandOption,
            ApplicationCommandOptionProperties creationOption)
        {
            List<IApplicationCommandOptionChoice> commandChoices = commandOption.Choices?.ToList() ?? new List<IApplicationCommandOptionChoice>();
            List<ApplicationCommandOptionChoiceProperties> creationChoices = creationOption.Choices ?? new List<ApplicationCommandOptionChoiceProperties>();

            if (commandChoices.Count != creationChoices.Count)
            {
                return false;
            }

            for (int i = 0; i < commandChoices.Count; i++)
            {
                IApplicationCommandOptionChoice commandChoice = commandChoices[i];
                ApplicationCommandOptionChoiceProperties creationChoice = creationChoices[i];

                if (commandChoice.Name != creationChoice.Name || commandChoice.Value != creationChoice.Value)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool MatchesCreationOptions(List<ApplicationCommandOptionProperties> creationOptions, List<IApplicationCommandOption> commandOptions)
        {
            if (creationOptions.Count != commandOptions.Count)
            {
                return false;
            }
            
            for (int i = 0; i < creationOptions.Count; i++)
            {
                ApplicationCommandOptionProperties creationOption = creationOptions[i];
                IApplicationCommandOption commandOption = commandOptions[i];

                if (!commandOption.MatchesCreationOption(creationOption))
                {
                    return false;
                }
            }

            return true;
        }
    }
}