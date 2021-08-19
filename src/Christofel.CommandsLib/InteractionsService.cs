using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Lifetime;
using Christofel.BaseLib.Permissions;
using Christofel.BaseLib.Plugins;
using Christofel.CommandsLib.CommandsInfo;
using Discord.Net.Interactions;
using Discord.Net.Interactions.Abstractions;
using Discord.Net.Interactions.Handlers;

namespace Christofel.CommandsLib
{
    public class InteractionsService : InteractionsService<PermissionSlashInfo>, IStartable, IRefreshable, IStoppable
    {
        private readonly IPermissionService _permissionService;
        private readonly List<IPermission> _commandPermissions;
        private readonly IApplicationLifetime _lifetime;

        public InteractionsService(InteractionHandler<PermissionSlashInfo> interactionHandler,
            ICommandHolder<PermissionSlashInfo> commandHolder,
            ICommandsRegistrator<PermissionSlashInfo> commandsRegistrator,
            ICommandsGroupProvider<PermissionSlashInfo> commandsGroupProvider,
            IPermissionService permissionService,
            IApplicationLifetime lifetime) : base(interactionHandler,
            commandHolder, commandsRegistrator, commandsGroupProvider)
        {
            _permissionService = permissionService;
            _commandPermissions = new List<IPermission>();
            _lifetime = lifetime;
        }

        public override async Task StartAsync(CancellationToken token = new CancellationToken())
        {
            await base.StartAsync(token);

            foreach (HeldSlashCommand<PermissionSlashInfo> heldCommand in _commandHolder.Commands)
            {
                var commandPermission = new CommandPermission(heldCommand.Info.BuiltCommand, heldCommand.Info.Permission);
                
                _commandPermissions.Add(commandPermission);
                _permissionService.RegisterPermission(commandPermission);
            }
        }

        public override async Task StopAsync(CancellationToken token = new CancellationToken())
        {
            if (_lifetime.State < LifetimeState.Stopping)
            {
                await base.StopAsync(token);
            }
            else
            {
                await _interactionHandler.StopAsync(token);
            }

            foreach (IPermission commandPermission in _commandPermissions)
            {
                _permissionService.UnregisterPermission(commandPermission);
            }
            
            _commandPermissions.Clear();
        }
    }
}