using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Remora.Commands.Trees;
using Remora.Commands.Trees.Nodes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Results;
using Remora.Discord.Commands.Services;
using Remora.Discord.Core;
using Remora.Results;

namespace Christofel.CommandsLib
{
    public class ChristofelSlashService
    {
        private record CommandInfo(IBulkApplicationCommandData Data, bool DefaultPermission,
            IReadOnlyList<IApplicationCommandPermissions> Permissions);

        private readonly CommandTree _commandTree;
        private readonly IDiscordRestOAuth2API _oauth2API;
        private readonly IDiscordRestApplicationAPI _applicationAPI;
        private readonly ChristofelCommandPermissionResolver _permissionResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlashService"/> class.
        /// </summary>
        /// <param name="commandTree">The command tree.</param>
        /// <param name="oauth2API">The OAuth2 API.</param>
        /// <param name="applicationAPI">The application API.</param>
        /// <param name="permissionResolver"></param>
        public ChristofelSlashService
        (
            CommandTree commandTree,
            IDiscordRestOAuth2API oauth2API,
            IDiscordRestApplicationAPI applicationAPI,
            ChristofelCommandPermissionResolver permissionResolver
        )
        {
            _permissionResolver = permissionResolver;
            _commandTree = commandTree;
            _applicationAPI = applicationAPI;
            _oauth2API = oauth2API;
        }

        /// <summary>
        /// Determines whether the application's commands support being bound to Discord slash commands.
        /// </summary>
        /// <returns>true if slash commands are supported; otherwise, false.</returns>
        public Result SupportsSlashCommands()
        {
            var couldCreate = _commandTree.CreateApplicationCommands();

            return couldCreate.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(couldCreate);
        }

        /// <summary>
        /// Updates the application's slash commands.
        /// </summary>
        /// <param name="guildID">The ID of the guild to update slash commands in, if any.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A result which may or may not have succeeded.</returns>
        public async Task<Result> UpdateSlashCommandsAsync
        (
            Snowflake guildID,
            CancellationToken ct = default
        )
        {
            var getApplication = await _oauth2API.GetCurrentBotApplicationInformationAsync(ct);
            if (!getApplication.IsSuccess)
            {
                return Result.FromError(getApplication);
            }

            var application = getApplication.Entity;
            var createCommands = _commandTree.CreateApplicationCommands();
            if (!createCommands.IsSuccess)
            {
                return Result.FromError(createCommands);
            }

            var mappedCommands = await MapCommandsAsync(guildID, createCommands.Entity, ct);

            var createdCommands = await _applicationAPI.GetGuildApplicationCommandsAsync(application.ID, guildID, ct);

            if (!createCommands.IsSuccess)
            {
                return Result.FromError(createCommands.Error);
            }

            var guildCommandPermissions =
                await _applicationAPI.GetGuildApplicationCommandPermissionsAsync(application.ID, guildID, ct);

            if (!guildCommandPermissions.IsSuccess)
            {
                return Result.FromError(guildCommandPermissions.Error);
            }

            foreach (var command in mappedCommands)
            {
                var result = await CreateOrModifyCommandAsync(application.ID, guildID, command, createdCommands.Entity,
                    guildCommandPermissions.Entity, ct);

                if (!result.IsSuccess)
                {
                    return Result.FromError(result.Error);
                }
            }

            return Result.FromSuccess();
        }

        public async Task<Result> DeleteSlashCommandsAsync
        (
            Snowflake guildID,
            CancellationToken ct = default
        )
        {
            var getApplication = await _oauth2API.GetCurrentBotApplicationInformationAsync(ct);
            if (!getApplication.IsSuccess)
            {
                return Result.FromError(getApplication);
            }

            var application = getApplication.Entity;
            var deleteCommands = _commandTree.CreateApplicationCommands();
            if (!deleteCommands.IsSuccess)
            {
                return Result.FromError(deleteCommands);
            }

            var loadedCommands = await _applicationAPI.GetGuildApplicationCommandsAsync(application.ID, guildID, ct);

            if (!loadedCommands.IsSuccess)
            {
                return Result.FromError(loadedCommands.Error);
            }

            foreach (var commandInfo in deleteCommands.Entity)
            {
                var appCommand = loadedCommands.Entity.FirstOrDefault(x => x.Name == commandInfo.Name);
                if (appCommand is null)
                {
                    continue;
                }

                var result =
                    await _applicationAPI.DeleteGuildApplicationCommandAsync(application.ID, guildID, appCommand.ID,
                        ct);

                if (!result.IsSuccess)
                {
                    return Result.FromError(result.Error);
                }
            }

            return Result.FromSuccess();
        }

        private async Task<Result> CreateOrModifyCommandAsync
        (
            Snowflake applicationID,
            Snowflake guildID,
            CommandInfo command,
            IReadOnlyList<IApplicationCommand> createdCommands,
            IReadOnlyList<IGuildApplicationCommandPermissions> guildCommandPermissions,
            CancellationToken ct = default
        )
        {
            // TODO: split to more methods
            var registeredCommand = createdCommands.FirstOrDefault(x => x.Name == command.Data.Name);

            if (registeredCommand is null)
            {
                var result = await _applicationAPI.CreateGuildApplicationCommandAsync(
                    applicationID,
                    guildID,
                    command.Data.Name,
                    command.Data.Description.HasValue ? command.Data.Description.Value : "",
                    command.Data.Options,
                    command.DefaultPermission,
                    command.Data.Type,
                    ct);

                if (!result.IsSuccess)
                {
                    return Result.FromError(result.Error);
                }

                registeredCommand = result.Entity;
            }
            else if (!CommandMatches(registeredCommand, command.DefaultPermission, command.Data))
            {
                Optional<IReadOnlyList<IApplicationCommandOption>?> options = default;
                if (command.Data.Options.HasValue)
                {
                    options = new Optional<IReadOnlyList<IApplicationCommandOption>?>(command.Data.Options.Value);
                }

                var result = await _applicationAPI.EditGuildApplicationCommandAsync(
                    applicationID,
                    guildID,
                    registeredCommand.ID,
                    command.Data.Name,
                    command.Data.Description.HasValue ? command.Data.Description.Value : "",
                    options,
                    command.DefaultPermission,
                    ct);

                if (!result.IsSuccess)
                {
                    return Result.FromError(result.Error);
                }

                registeredCommand = result.Entity;
            }

            var commandPermissions = guildCommandPermissions.FirstOrDefault(x => x.ID == registeredCommand.ID);

            if (commandPermissions is null || !PermissionsMatch(command.Permissions, commandPermissions.Permissions))
            {
                var permissionsResult = await _applicationAPI.EditApplicationCommandPermissionsAsync(
                    applicationID,
                    guildID,
                    registeredCommand.ID,
                    command.Permissions,
                    ct
                );

                if (!permissionsResult.IsSuccess)
                {
                    return Result.FromError(permissionsResult.Error);
                }
            }

            return Result.FromSuccess();
        }

        private bool PermissionsMatch(IReadOnlyList<IApplicationCommandPermissions> left,
            IReadOnlyList<IApplicationCommandPermissions> right)
        {
            if (left.Count != right.Count)
            {
                return false;
            }

            using var leftPermissionEnumerator = left.OrderBy(x => x.ID.Value).GetEnumerator();
            using var rightPermissionEnumerator = right.OrderBy(x => x.ID.Value).GetEnumerator();

            while (leftPermissionEnumerator.MoveNext() && rightPermissionEnumerator.MoveNext())
            {
                var leftPermission = leftPermissionEnumerator.Current;
                var rightPermission = rightPermissionEnumerator.Current;

                if (leftPermission.ID != rightPermission.ID ||
                    leftPermission.HasPermission != rightPermission.HasPermission ||
                    leftPermission.Type != rightPermission.Type)
                {
                    return false;
                }
            }

            return true;
        }

        private bool CommandMatches(IApplicationCommand command, bool defaultPermission,
            IBulkApplicationCommandData commandData)
        {
            if (command.Name != commandData.Name ||
                command.Description != commandData.Description && command.Type != commandData.Type ||
                ((command.DefaultPermission.HasValue ? command.DefaultPermission : false) != defaultPermission) ||
                !HasSameLength(commandData.Options, command.Options))
            {
                return false;
            }

            return !command.Options.HasValue || !commandData.Options.HasValue ||
                   CommandOptionsMatch(command.Options.Value, commandData.Options.Value);
        }

        private bool CommandOptionsMatch(IReadOnlyList<IApplicationCommandOption> left,
            IReadOnlyList<IApplicationCommandOption> right)
        {
            if (left.Count != right.Count)
            {
                return false;
            }

            using var leftOptionEnumerator = left.OrderBy(x => x.Name).GetEnumerator();
            using var rightOptionEnumerator = right.OrderBy(x => x.Name).GetEnumerator();

            while (leftOptionEnumerator.MoveNext() && rightOptionEnumerator.MoveNext())
            {
                var leftOption = leftOptionEnumerator.Current;
                var rightOption = rightOptionEnumerator.Current;

                if (!CommandOptionMatches(leftOption, rightOption))
                {
                    return false;
                }
            }

            return true;
        }

        private bool CommandChoicesMatch(IReadOnlyList<IApplicationCommandOptionChoice> left,
            IReadOnlyList<IApplicationCommandOptionChoice> right)
        {
            if (left.Count != right.Count)
            {
                return false;
            }

            using var leftChoiceEnumerator = left.OrderBy(x => x.Name).GetEnumerator();
            using var rightChoiceEnumerator = right.OrderBy(x => x.Name).GetEnumerator();

            while (leftChoiceEnumerator.MoveNext() && rightChoiceEnumerator.MoveNext())
            {
                var leftChoice = leftChoiceEnumerator.Current;
                var rightChoice = rightChoiceEnumerator.Current;

                if (!leftChoice.Value.Equals(rightChoice.Value) || leftChoice.Name != rightChoice.Name)
                {
                    return false;
                }
            }

            return true;
        }

        private bool HasSameLength<T>(Optional<IReadOnlyList<T>> left, Optional<IReadOnlyList<T>> right)
        {
            if ((!left.HasValue && right.HasValue && right.Value.Count == 0) ||
                (!right.HasValue && left.HasValue && left.Value.Count == 0))
            {
                return true;
            }

            if (left.HasValue != right.HasValue)
            {
                return false;
            }

            return !left.HasValue || (left.Value.Count == right.Value.Count);
        }

        private bool CheckOptionalMatches(Optional<bool> left, Optional<bool> right, bool @default)
        {
            return (left.HasValue ? left.Value : @default) == (right.HasValue ? right.Value : @default);
        }

        private bool CommandOptionMatches(IApplicationCommandOption left,
            IApplicationCommandOption right)
        {
            if (left.Name != right.Name || left.Description != right.Description || left.Type != right.Type ||
                !CheckOptionalMatches(left.IsDefault, right.IsDefault, false) ||
                !CheckOptionalMatches(left.IsRequired, right.IsRequired, false) ||
                !HasSameLength(left.Options, right.Options) || !HasSameLength(left.Choices, right.Choices))
            {
                return false;
            }

            if (left.Options.HasValue && right.Options.HasValue &&
                !CommandOptionsMatch(left.Options.Value, right.Options.Value))
            {
                return false;
            }

            return !left.Choices.HasValue || !right.Choices.HasValue ||
                   CommandChoicesMatch(left.Choices.Value, right.Choices.Value);
        }

        private async Task<IReadOnlyCollection<CommandInfo>>
            MapCommandsAsync
            (
                Snowflake? guildId,
                IReadOnlyList<IBulkApplicationCommandData> commands,
                CancellationToken ct = default
            )
        {
            var returnData = new List<CommandInfo>();

            foreach (var rootCommand in _commandTree.Root.Children)
            {
                bool defaultPermission = false;
                IBulkApplicationCommandData commandData;
                IEnumerable<IApplicationCommandPermissions> permissions;

                switch (rootCommand)
                {
                    case CommandNode commandNode:
                        commandData = commands.First(x => x.Name == commandNode.Key);
                        permissions = await _permissionResolver.GetCommandPermissionsAsync(guildId, commandNode, ct);
                        defaultPermission = await _permissionResolver.IsForEveryoneAsync(guildId, commandNode, ct);
                        break;
                    case GroupNode groupNode:
                        commandData = commands.First(x => x.Name == groupNode.Key);
                        permissions = await _permissionResolver.GetCommandPermissionsAsync(guildId, groupNode, ct);
                        defaultPermission = await _permissionResolver.IsForEveryoneAsync(guildId, groupNode, ct);
                        break;
                    default:
                        throw new InvalidOperationException("Invalid root type");
                    // Handle shomehow?
                }

                commandData = new BulkApplicationCommandData(commandData.Name,
                    commandData.Description.HasValue ? commandData.Description : string.Empty, commandData.Options,
                    commandData.DefaultPermission.HasValue ? commandData.DefaultPermission : false, commandData.Type);
                returnData.Add(new CommandInfo(commandData, defaultPermission, permissions.ToList()));
            }

            return returnData;
        }
    }
}