using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Permissions;
using Christofel.CommandsLib.CommandsInfo;
using Christofel.CommandsLib.Handlers;
using Discord.Rest;
using Discord.WebSocket;

namespace Christofel.CommandsLib.Commands
{
    public class CommandRegistrator : ICommandsRegistrator
    {
        private readonly  ICommandsGroupProvider _commandGroups;
        private readonly ICommandHolder _commandsHolder;
        private readonly IPermissionService _permissions;
        private readonly DiscordRestClient _client;

        public CommandRegistrator(ICommandsGroupProvider commandGroups, ICommandHolder commandsHolder, IPermissionService permissions, DiscordRestClient client)
        {
            _commandGroups = commandGroups;
            _commandsHolder = commandsHolder;
            _permissions = permissions;
            _client = client;
        }
        
        public async Task StartAsync(CancellationToken token = new CancellationToken())
        {
            await Task.WhenAll(
                _commandGroups.GetGroups().Select(x => x.SetupCommandsAsync(_commandsHolder, token)));

            await RegisterCommandsAsync(_commandsHolder, token);
        }

        public Task StopAsync(CancellationToken token = new CancellationToken())
        {
            return UnregisterCommandsAsync(_commandsHolder, token);
        }

        public Task RefreshAsync(CancellationToken token = new CancellationToken())
        {
            return RefreshCommandsAndPermissionsAsync(_commandsHolder, token);
        }

        public async Task RegisterCommandsAsync(ICommandHolder holder, CancellationToken token = default)
        {
            foreach (ICommandHolder.HeldSlashCommand heldCommand in holder.Commands)
            {
                await heldCommand.Info.RegisterCommandAsync(_client, _permissions.Resolver, token);
                await heldCommand.Info.RegisterPermissionsAsync(_client, _permissions, token);
            }
        }

        public Task UnregisterCommandsAsync(ICommandHolder holder, CancellationToken token = default)
        {
            return Task.WhenAll(
                holder.Commands
                    .Select(x => x.Info.UnregisterCommandAsync(_permissions, token)));
            
        }

        public Task RefreshCommandsAndPermissionsAsync(ICommandHolder holder, CancellationToken token = default)
        {
            return Task.WhenAll(
                holder.Commands.Select(x => x.Info.RefreshCommandAndPermissionsAsync(_permissions.Resolver, token)));
        }
    }
}