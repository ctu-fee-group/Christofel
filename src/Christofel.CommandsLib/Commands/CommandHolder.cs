using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Permissions;
using Christofel.CommandsLib.CommandsInfo;
using Christofel.CommandsLib.Executors;
using Christofel.CommandsLib.Extensions;
using Discord;
using Discord.Rest;

namespace Christofel.CommandsLib.Commands
{
    /// <summary>
    /// Thread-safe implementation of CommandHolder
    /// </summary>
    public class CommandHolder : ICommandHolder
    {
        private readonly List<ICommandHolder.HeldSlashCommand> _commands;
        private readonly DiscordRestClient _client;
        private readonly IPermissionService _permissions;
        private readonly object _commandsLock = new object();
        
        public CommandHolder(DiscordRestClient client, IPermissionService permissions)
        {
            _client = client;
            _permissions = permissions;
            _commands = new List<ICommandHolder.HeldSlashCommand>();
        }

        public Task RefreshCommandsAndPermissionsAsync(CancellationToken token = default)
        {
            return Task.WhenAll(
                _commands.Select(x => x.Info.RefreshCommandAndPermissionsAsync(_permissions.Resolver, token)));
        }

        public ICommandHolder.HeldSlashCommand? TryGetSlashCommand(string name)
        {
            return _commands.FirstOrDefault(x => x.Info.Builder.Name == name);
        }

        public async Task<SlashCommandInfo> RegisterCommandAsync(SlashCommandBuilder builder, ICommandExecutor executor, CancellationToken token = default)
        {
            SlashCommandInfo info = builder.BuildAndGetInfo();
            await info.RegisterCommandAsync(_client, _permissions.Resolver, token);
            await info.RegisterPermissionsAsync(_client, _permissions, token);

            lock (_commandsLock)
            {
                _commands.Add(new ICommandHolder.HeldSlashCommand(info, executor));
            }

            return info;
        }

        public Task UnregisterCommandsAsync(CancellationToken token = new CancellationToken())
        {
            List<ICommandHolder.HeldSlashCommand> commands = _commands.ToList();
            lock (_commandsLock)
            {
                _commands.Clear();
            }

            return Task.WhenAll(
                commands
                    .Select(x => x.Info.UnregisterCommandAsync(_permissions, token)));
        }
    }
}