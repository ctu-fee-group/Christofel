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
            Snowflake? guildID = null,
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

            foreach (var command in mappedCommands)
            {
                // TODO: update only if it does not match the current commands
                
                if (guildID is null)
                {
                    var result = await _applicationAPI.CreateGlobalApplicationCommandAsync(
                        application.ID,
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
                }
                else
                {
                    var result = await _applicationAPI.CreateGuildApplicationCommandAsync(
                        application.ID,
                        (Snowflake)guildID,
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

                    var permissionsResult = await _applicationAPI.EditApplicationCommandPermissionsAsync(
                        application.ID,
                        (Snowflake)guildID,
                        result.Entity.ID,
                        command.Permissions,
                        ct
                    );
                    
                    if (!permissionsResult.IsSuccess)
                    {
                        return Result.FromError(permissionsResult.Error);
                    }
                }
            }

            return Result.FromSuccess();
        }

        public async Task<Result> DeleteSlashCommandsAsync
        (
            Snowflake? guildID = null,
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

            Result<IReadOnlyList<IApplicationCommand>> loadedCommands;

            if (guildID is null)
            {
                loadedCommands = await _applicationAPI.GetGlobalApplicationCommandsAsync(application.ID, ct);
            }
            else
            {
                loadedCommands = await _applicationAPI.GetGuildApplicationCommandsAsync(application.ID, (Snowflake)guildID, ct);
            }

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

                Result result;
                if (guildID is null)
                {
                    result = await _applicationAPI.DeleteGlobalApplicationCommandAsync(application.ID, appCommand.ID, ct);
                }
                else
                {
                    result = await _applicationAPI.DeleteGuildApplicationCommandAsync(application.ID, (Snowflake)guildID, appCommand.ID, ct);
                }

                if (!result.IsSuccess)
                {
                    return Result.FromError(result.Error);
                }
            }
            
            return Result.FromSuccess();
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
                        continue; // Handle shomehow?
                }

                returnData.Add(new CommandInfo(commandData, defaultPermission, permissions.ToList()));
            }

            return returnData;
        }
    }
}