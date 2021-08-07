using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Permissions;
using Christofel.BaseLib.Plugins;
using Christofel.CommandsLib.CommandsInfo;
using Christofel.CommandsLib.Extensions;
using Christofel.CommandsLib.Handlers;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace Christofel.CommandsLib.Commands
{
    /// <summary>
    /// Command registrator registering commands one by one
    /// </summary>
    public class CommandsRegistrator : ICommandsRegistrator, IStartable, IRefreshable, IStoppable
    {
        private readonly ICommandsGroupProvider _commandGroups;
        private readonly ICommandHolder _commandsHolder;
        private readonly IPermissionService _permissions;
        private readonly DiscordRestClient _client;
        private CommandCache _cache;

        public CommandsRegistrator(ICommandsGroupProvider commandGroups, ICommandHolder commandsHolder,
            IPermissionService permissions, DiscordRestClient client)
        {
            _commandGroups = commandGroups;
            _commandsHolder = commandsHolder;
            _permissions = permissions;
            _client = client;
            _cache = new CommandCache(client);
        }

        public async Task StartAsync(CancellationToken token = new CancellationToken())
        {
            _cache.Reset();
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(
                _commandGroups.GetGroups().Select(x => x.SetupCommandsAsync(_commandsHolder, token)));

            await RegisterCommandsAsync(_commandsHolder, token);
        }

        public Task StopAsync(CancellationToken token = new CancellationToken())
        {
            _cache.Reset();
            return UnregisterCommandsAsync(_commandsHolder, token);
        }

        public Task RefreshAsync(CancellationToken token = new CancellationToken())
        {
            _cache.Reset();
            return RefreshCommandsAndPermissionsAsync(_commandsHolder, token);
        }

        public async Task RegisterCommandsAsync(ICommandHolder holder, CancellationToken token = default)
        {
            foreach (ICommandHolder.HeldSlashCommand heldCommand in holder.Commands)
            {
                try
                {
                    await RegisterCommandAsync(heldCommand.Info, token);
                    _permissions.RegisterPermission(heldCommand.Info.Permission);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(
                        $@"Could not register a command {heldCommand.Info.BuiltCommand.Name}", e);
                }
            }
        }

        public Task UnregisterCommandsAsync(ICommandHolder holder, CancellationToken token = default)
        {
            return Task.WhenAll(
                holder.Commands
                    .Select(x => UnregisterCommandAsync(x.Info, token)));
        }

        public Task RefreshCommandsAndPermissionsAsync(ICommandHolder holder, CancellationToken token = default)
        {
            return Task.WhenAll(
                holder.Commands.Select(x => RefreshCommandAsync(x.Info, token)));
        }

        private async Task RefreshCommandAsync(SlashCommandInfo info, CancellationToken token = new CancellationToken())
        {
            if (info.Command == null)
            {
                throw new InvalidOperationException("Cannot refresh without the command registered");
            }

            await ModifyCommand(info, token);
            await RefreshPermissions(info, token);
        }

        private async Task<IApplicationCommand> RegisterCommandAsync(SlashCommandInfo info,
            CancellationToken token = new CancellationToken())
        {
            if (info.Command == null)
            {
                SlashCommandCreationProperties command = await SetDefaultPermissionAsync(info, token);

                if (info.Global)
                {
                    info.Command = await CreateGlobalCommand(info, token);
                }
                else
                {
                    info.Command = await CreateGuildCommand(info, token);
                }
            }

            info.Registered = true;
            return info.Command;
        }

        private async Task UnregisterCommandAsync(SlashCommandInfo info,
            CancellationToken token = new CancellationToken())
        {
            _permissions.UnregisterPermission(info.Permission);
            if (info.Command != null)
            {
                await info.Command.DeleteAsync(new()
                {
                    CancelToken = token
                });
                info.Command = null;
            }

            info.Registered = false;
        }

        private async Task ModifyCommand(SlashCommandInfo info, CancellationToken token = new CancellationToken())
        {
            if (info.Command is RestGlobalCommand globalCommand &&
                !info.Command.MatchesCreationProperties(info.BuiltCommand))
            {
                await globalCommand.ModifyAsync(props =>
                    ModifyCommandProperties(info, props, token).GetAwaiter().GetResult());
            }
            else if (info.Command is RestGuildCommand guildCommand)
            {
                if (!info.Command.MatchesCreationProperties(info.BuiltCommand))
                {
                    await guildCommand.ModifyAsync(props =>
                        ModifyCommandProperties(info, props, token).GetAwaiter().GetResult());
                }

                await RefreshPermissions(info, token);
            }
        }

        private async Task RefreshPermissions(SlashCommandInfo info, CancellationToken token = new CancellationToken())
        {
            if (info.Command is RestGlobalCommand)
            {
                return; // Global commands cannot have permissions (at least not in Discord.NET yet)
            }
            else if (info.Command is RestGuildCommand guildCommand)
            {
                ApplicationCommandPermission[] permissions =
                    await _permissions.Resolver.GetSlashCommandPermissionsAsync(info.Permission, token);
                GuildApplicationCommandPermission? commandPermission = await guildCommand.GetCommandPermission();

                if (commandPermission == null || !commandPermission.MatchesPermissions(permissions))
                {
                    await guildCommand.ModifyCommandPermissions(permissions);
                }
            }
        }

        private async Task<SlashCommandCreationProperties> SetDefaultPermissionAsync(
            SlashCommandInfo info,
            CancellationToken token = new CancellationToken())
        {
            info.BuiltCommand.DefaultPermission = await info.IsForEveryoneAsync(_permissions.Resolver, token);
            return info.BuiltCommand;
        }

        private async Task<ApplicationCommandProperties> ModifyCommandProperties(
            SlashCommandInfo info,
            ApplicationCommandProperties command,
            CancellationToken token = new CancellationToken())
        {
            command.Description = info.BuiltCommand.Description;
            command.Name = info.BuiltCommand.Name;
            command.Options = info.BuiltCommand.Options;
            command.DefaultPermission = await info.IsForEveryoneAsync(_permissions.Resolver, token);
            return command;
        }

        private async Task<RestApplicationCommand> CreateGlobalCommand(SlashCommandInfo info,
            CancellationToken token = new CancellationToken())
        {
            RestGlobalCommand? globalCommand = await _cache.GetGlobalCommand(info.BuiltCommand.Name, token);

            if (globalCommand != null)
            {
                info.Command = globalCommand;
                await ModifyCommand(info, token);
            }
            else
            {
                globalCommand = await _client.CreateGlobalCommand(info.BuiltCommand, new RequestOptions()
                {
                    CancelToken = token
                });
                info.Command = globalCommand;

                await RefreshPermissions(info, token);
            }

            return globalCommand;
        }

        private async Task<RestApplicationCommand> CreateGuildCommand(SlashCommandInfo info,
            CancellationToken token = new CancellationToken())
        {
            if (info.GuildId == null)
            {
                throw new ArgumentException("GuildId cannot be null for guild commands");
            }

            RestGuildCommand? guildCommand =
                await _cache.GetGuildCommand((ulong) info.GuildId, info.BuiltCommand.Name, token);

            if (guildCommand != null)
            {
                info.Command = guildCommand;
                await ModifyCommand(info, token);
            }
            else
            {
                guildCommand = await _client.CreateGuildCommand(info.BuiltCommand, (ulong) info.GuildId,
                    new RequestOptions()
                    {
                        CancelToken = token
                    });
                info.Command = guildCommand;

                await RefreshPermissions(info, token);
            }

            return guildCommand;
        }
    }
}