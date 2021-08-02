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
        private readonly object _commandsLock = new object();
        
        public CommandHolder(DiscordRestClient client, IPermissionService permissions)
        {
            _commands = new List<ICommandHolder.HeldSlashCommand>();
        }

        public IEnumerable<ICommandHolder.HeldSlashCommand> Commands => _commands.AsReadOnly();

        public ICommandHolder.HeldSlashCommand? TryGetSlashCommand(string name)
        {
            return _commands.FirstOrDefault(x => x.Info.Builder.Name == name);
        }

        public SlashCommandInfo AddCommand(SlashCommandBuilder builder, ICommandExecutor executor, CancellationToken token = default)
        {
            SlashCommandInfo info = builder.BuildAndGetInfo();

            lock (_commandsLock)
            {
                _commands.Add(new ICommandHolder.HeldSlashCommand(info, executor));
            }

            return info;
        }

        public void RemoveCommands(CancellationToken token = new CancellationToken())
        {
            List<ICommandHolder.HeldSlashCommand> commands = _commands.ToList();
            lock (_commandsLock)
            {
                _commands.Clear();
            }
        }
    }
}