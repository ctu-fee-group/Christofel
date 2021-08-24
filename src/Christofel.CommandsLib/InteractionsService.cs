using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Lifetime;
using Christofel.BaseLib.Permissions;
using Christofel.BaseLib.Plugins;
using Christofel.CommandsLib.CommandsInfo;

namespace Christofel.CommandsLib
{
    public class InteractionsService : Discord.Net.Interactions.InteractionsService, IStartable, IRefreshable,
        IStoppable
    {
        private readonly IPermissionService _permissionService;
        private readonly List<IPermission> _commandPermissions;
        private readonly IApplicationLifetime _lifetime;

        public InteractionsService(InteractionHandler interactionHandler, IInteractionHolder interactionHolder,
            ICommandsRegistrator commandsRegistrator, ICommandsGroupProvider commandsGroupProvider,
            IPermissionService permissionService,
            IApplicationLifetime lifetime) : base(
            interactionHandler, interactionHolder, commandsRegistrator, commandsGroupProvider)
        {
            _commandPermissions = new List<IPermission>();
            _permissionService = permissionService;
            _lifetime = lifetime;
        }

        public override async Task StartAsync(bool registerCommands = true, CancellationToken token = new CancellationToken())
        {
            await base.StartAsync(registerCommands, token);

            foreach (PermissionSlashInfo heldInteraction in _interactionHolder.Interactions.Select(x => x.Info)
                .OfType<PermissionSlashInfo>())
            {
                var commandPermission =
                    new CommandPermission(heldInteraction.BuiltCommand, heldInteraction.Permission);

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

        public Task StartAsync(CancellationToken token = new CancellationToken())
        {
            return StartAsync(true, token);
        }
    }
}